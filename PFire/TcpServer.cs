﻿using PFire.Protocol;
using PFire.Protocol.XFireAttributes;
using PFire.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PFire
{
    public class TcpServer
    {
        public delegate void OnRecieveHandler(Context sessionContext, IMessage message);
        public event OnRecieveHandler OnReceive;

        public delegate void OnConnectionHandler(Context sessionContext);
        public event OnConnectionHandler OnConnection;

        private TcpListener listener;

        public TcpServer(IPEndPoint endPoint)
        {
            listener = new TcpListener(endPoint);
        }

        public TcpServer(IPAddress ip, int port) : this(new IPEndPoint(ip, port)) { }

        public async void StartListening()
        {
            listener.Start();
            while (true)
            {
                Context session = new Context(await listener.AcceptTcpClientAsync().ConfigureAwait(false));
                Debug.WriteLine("Client connected {0} and assigned session id {1}", session.TcpClient.Client.RemoteEndPoint, session.SessionId);

                if (OnConnection != null)
                {
                    OnConnection(session);
                }

                Receive(session);
            }
        }

        private async Task Receive(Context context)
        {
            var stream = context.TcpClient.GetStream();
            while (true)
            {
                // First time the client connects, an opening statement of 4 bytes is sent that needs to be ignored
                if (!context.Initialized)
                {
                    var openingStatementBuffer = new byte[4];
                    stream.Read(openingStatementBuffer, 0, openingStatementBuffer.Length);
                    context.InitializeClient();
                }

                // Header determines size of message
                var headerBuffer = new byte[2];
                var read = await stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
                if (read == 0)
                {
                    break;
                }

                var messageLength = BitConverter.ToInt16(headerBuffer, 0) - headerBuffer.Length;
                var messageBuffer = new byte[messageLength];
                read = await stream.ReadAsync(messageBuffer, 0, messageLength);

                //Debug.WriteLine("RECEIVED RAW: " + BitConverter.ToString(messageBuffer));

                try
                {
                    IMessage message = MessageSerializer.Deserialize(messageBuffer);
                    if (OnReceive != null)
                    {
                        OnReceive(context, message);
                    }
                }
                catch (UnknownMessageTypeException messageTypeEx)
                {
                    Debug.WriteLine(messageTypeEx.ToString());
                }
                catch (UnknownXFireAttributeTypeException attributeTypeEx)
                {
                    Debug.WriteLine(attributeTypeEx.ToString());
                }
            }
        }
    }
}