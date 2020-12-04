using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PFire.Core.Services
{
    internal interface IXFireClientProvider
    {
        IXFireClient GetClient(TcpClient tcpClient, Func<IXFireClient, Task> disconnectionHandler);
    }

    internal sealed class XFireClientProvider : IXFireClientProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public XFireClientProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IXFireClient GetClient(TcpClient tcpClient, Func<IXFireClient, Task> disconnectionHandler)
        {
            var xFireClient = _serviceProvider.GetRequiredService<XFireClient>();
            xFireClient.Init(tcpClient, disconnectionHandler);

            return xFireClient;
        }
    }
}
