using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PFire.Common.Models;
using PFire.Core.Services;

namespace PFire.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCore(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<IPFireServer, PFireServer>()
                                    .AddSingleton<IXFireClientManager, XFireClientManager>()
                                    .AddTransient<IXFireTcpListener, XFireTcpListener>()
                                    .AddSingleton<IPFireDatabase, PFireDatabase>()
                                    .AddSingleton<IXFireMessageProcessor, XFireMessageProcessor>()
                                    .AddSingleton<IMessageSerializer, MessageSerializer>()
                                    .AddSingleton<IXFireClientProvider, XFireClientProvider>()
                                    .AddSingleton<IMessageFactory, MessageFactory>()
                                    .AddTransient<XFireClient>()
                                    .AddSingleton(x =>
                                    {
                                        var serverSettings = x.GetRequiredService<IOptions<ServerSettings>>().Value;
                                        return new IPEndPoint(IPAddress.Any, serverSettings.Port);
                                    })
                                    .AddSingleton<TcpListener>();
        }
    }
}
