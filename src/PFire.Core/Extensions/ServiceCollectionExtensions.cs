using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PFire.Common.Models;
using PFire.Core.Services;
using PFire.Core.Session;

namespace PFire.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCore(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IPFireServer, PFireServer>()
                                    .AddSingleton<IXFireClientManager, XFireClientManager>()
                                    .AddSingleton<ITcpServer, TcpServer>()
                                    .AddSingleton<IPFireDatabase, PFireDatabase>()
                                    .AddSingleton(x =>
                                    {
                                        var serverSettings = x.GetRequiredService<IOptions<ServerSettings>>().Value;

                                        return new TcpListener(IPAddress.Any, serverSettings.Port);
                                    });
        }
    }
}
