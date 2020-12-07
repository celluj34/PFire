using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PFire.Core.Session;

namespace PFire.Core
{
    internal sealed class TcpServer : ITcpServer
    {
        private readonly IXFireClientManager _clientManager;
        private readonly TcpListener _listener;
        private readonly ILogger<TcpServer> _logger;

        public TcpServer(TcpListener listener, IXFireClientManager clientManager, ILogger<TcpServer> logger)
        {
            _listener = listener;
            _clientManager = clientManager;
            _logger = logger;
        }

        public event ITcpServer.OnReceiveHandler OnReceive;
        public event ITcpServer.OnConnectionHandler OnConnection;
        public event ITcpServer.OnDisconnectionHandler OnDisconnection;

        public async Task Listen(CancellationToken cancellationToken)
        {
            _listener.Start();
            _logger.LogInformation($"PFire Server listening on {_listener.LocalEndpoint}");

            //Stop listener when cancellationToken is cancelled
            cancellationToken.Register(() => _listener.Stop());

            while (true)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                var newXFireClient = new XFireClient(tcpClient, _clientManager, _logger, OnReceive, OnDisconnection);

                OnConnection?.Invoke(newXFireClient);
            }
        }
    }
}
