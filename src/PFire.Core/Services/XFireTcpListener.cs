using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PFire.Core.Services
{
    internal interface IXFireTcpListener
    {
        EndPoint LocalEndpoint { get; }
        Task Start();
        Task Stop();
        Task<TcpClient> AcceptTcpClientAsync();
    }

    internal sealed class XFireTcpListener : IXFireTcpListener
    {
        private readonly TcpListener _tcpListener;

        public XFireTcpListener(TcpListener tcpListener)
        {
            _tcpListener = tcpListener;
        }

        public async Task Start()
        {
            await Task.Yield();

            _tcpListener.Start();
        }

        public async Task Stop()
        {
            await Task.Yield();

            _tcpListener.Stop();
        }

        public EndPoint LocalEndpoint => _tcpListener.LocalEndpoint;

        public Task<TcpClient> AcceptTcpClientAsync()
        {
            return _tcpListener.AcceptTcpClientAsync();
        }
    }
}
