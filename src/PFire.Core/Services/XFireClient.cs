using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PFire.Core.Exceptions;
using PFire.Core.Messages;
using PFire.Core.Models;
using PFire.Core.Util;

namespace PFire.Core.Services
{
    internal interface IXFireClient
    {
        UserModel User { get; set; }
        Guid SessionId { get; }
        int PublicIp { get; }
        string Salt { get; }
        void Disconnect();
        void Dispose();
        Task SendAndProcessMessage(IMessage message);
        Task SendMessage(IMessage message);
        void RemoveOtherSessions(UserModel user);
    }

    internal sealed class XFireClient : Disposable, IXFireClient
    {
        private const int ClientTimeoutInMinutes = 5;

        private readonly IXFireClientManager _clientManager;
        private readonly ILogger<XFireClient> _logger;
        private readonly IXFireMessageProcessor _messageProcessor;
        private readonly IMessageSerializer _messageSerializer;
        private bool _connected;
        private Func<IXFireClient, Task> _disconnectionHandler;
        private DateTime _lastReceivedFrom;
        private TcpClient _tcpClient;

        public XFireClient(IXFireClientManager clientManager,
                           ILogger<XFireClient> logger,
                           IXFireMessageProcessor messageProcessor,
                           IMessageSerializer messageSerializer)
        {
            _clientManager = clientManager;
            _logger = logger;
            _messageProcessor = messageProcessor;
            _messageSerializer = messageSerializer;
        }

        public string Salt { get; private set; }

        public Guid SessionId { get; private set; }

        public UserModel User { get; set; }

        public int PublicIp
        {
            get
            {
                IPAddress address;
                var remoteEndPoint = _tcpClient.Client.RemoteEndPoint;
                if (remoteEndPoint is IPEndPoint ipEndPoint)
                {
                    address = ipEndPoint.Address;
                }
                else
                {
                    var addressStr = remoteEndPoint.ToString();
                    var ip = addressStr.Substring(0, addressStr.IndexOf(":"));
                    address = IPAddress.Parse(ip);
                }

                var addressBytes = address.GetAddressBytes();

                return BitConverter.ToInt32(addressBytes);
            }
        }

        public void Disconnect()
        {
            _connected = false;
        }

        public async Task SendAndProcessMessage(IMessage message)
        {
            await _messageProcessor.Process(message, this, _clientManager);
            await SendMessage(message);
        }

        public async Task SendMessage(IMessage message)
        {
            if (Disposed)
            {
                // He's dead, Jim.
                return;
            }

            var payload = _messageSerializer.Serialize(message);

            await _tcpClient.Client.SendAsync(payload, SocketFlags.None);

            var username = User?.Username ?? "unknown";
            var userId = User?.Id ?? -1;

            _logger.LogDebug($"Sent message[{username},{userId}]: {message}");
        }

        // A login has been successful, and as part of the login processing
        // we should remove any duplicate/old sessions
        public void RemoveOtherSessions(UserModel user)
        {
            var otherSession = _clientManager.GetSession(user);
            if (otherSession != null)
            {
                _clientManager.RemoveSession(otherSession);
            }
        }

        public void Init(TcpClient tcpClient, Func<IXFireClient, Task> disconnectionHandler)
        {
            _disconnectionHandler = disconnectionHandler;
            _tcpClient = tcpClient;
            _tcpClient.ReceiveTimeout = (int)TimeSpan.FromMinutes(ClientTimeoutInMinutes).TotalMilliseconds;

            _connected = true;

            // TODO: be able to use unique salts
            Salt = "4dc383ea21bf4bca83ea5040cb10da62";
            SessionId = Guid.NewGuid();

            _lastReceivedFrom = DateTime.UtcNow;

            _logger.LogInformation($"Client connected {_tcpClient.Client.RemoteEndPoint} and assigned session id {SessionId}");
        }

        protected override void DisposeManagedResources()
        {
            try
            {
                if (_tcpClient.Connected)
                {
                    _tcpClient.Close();
                }
                else
                {
                    _tcpClient.Dispose();
                }
            }
            finally
            {
                _tcpClient = null;
            }
        }

        private async void ClientThreadWorker()
        {
            while (_connected)
            {
                if (!_tcpClient.Connected)
                {
                    // the client says the other end has gone, 
                    // lets shut down this client 
                    _logger.LogError($"Client: {User.Username}-{SessionId} has disconnected");
                    await _disconnectionHandler(this);

                    return;
                }

                var messageBuffer = await GetMessage();
                if (messageBuffer == null)
                {
                    return;
                }

                try
                {
                    var message = _messageSerializer.Deserialize(messageBuffer);

                    var username = User?.Username ?? "unknown";
                    var userId = User?.Id ?? -1;

                    _logger.LogDebug($"Recv message[{username},{userId}]: {message}");

                    await _messageProcessor.Process(message, this, _clientManager);
                }
                catch (UnknownMessageTypeException messageTypeEx)
                {
                    _logger.LogDebug(messageTypeEx, "Unknown Message Type");
                }
                catch (UnknownXFireAttributeTypeException attributeTypeEx)
                {
                    _logger.LogDebug(attributeTypeEx, "Unknown XFireAttribute Type");
                }
            }
        }

        private async Task<byte[]> GetMessage()
        {
            try
            {
                var stream = _tcpClient.GetStream();

                // Header determines size of message
                var headerBuffer = new byte[2];
                var read = await stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
                if (read == 0)
                {
                    _logger.LogCritical($"Client {User?.Username}-{SessionId} disconnected via 0 read");
                    await _disconnectionHandler(this);
                    return null;
                }

                var messageLength = BitConverter.ToInt16(headerBuffer, 0) - headerBuffer.Length;
                var messageBuffer = new byte[messageLength];
                read = await stream.ReadAsync(messageBuffer, 0, messageLength);

                _logger.LogTrace($"RECEIVED RAW: {BitConverter.ToString(messageBuffer)}");

                // as we read something (i.e we're still here) we can update the last read time
                _lastReceivedFrom = DateTime.UtcNow;

                return messageBuffer;
            }
            catch (IOException ioe)
            {
                // the read timed out 
                // this could indicate that the other end is bad
                // the lifetime handler will help
                _logger.LogError(ioe, "An exception occurred when reading from the tcp stream");
                await _disconnectionHandler(this);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Client: {User.Username}-{SessionId} has timed out -> {_lastReceivedFrom}");
                await _disconnectionHandler(this);

                return null;
            }
        }

        public async Task<bool> ReadOpeningHeader()
        {
            var stream = _tcpClient.GetStream();

            // First time the client connects, an opening statement of 4 bytes is sent that needs to be ignored
            var openingStatementBuffer = new byte[4];
            var read = await stream.ReadAsync(openingStatementBuffer, 0, openingStatementBuffer.Length);

            if (read == 4)
            {
                return true;
            }

            _logger.LogError($"Failed to read header bytes from {SessionId}");

            return false;
        }

        public void BeginRead()
        {
            ThreadPool.QueueUserWorkItem(sender => ClientThreadWorker());
        }
    }
}
