using System;
using System.Threading;
using System.Threading.Tasks;
using PFire.Core.Protocol.Messages;
using PFire.Core.Session;

namespace PFire.Core
{
    internal interface ITcpServer
    {
        event Func<IXFireClient, IMessage, Task> OnReceive;
        event Func<IXFireClient, Task> OnConnection;
        event Func<IXFireClient, Task> OnDisconnection;

        Task Listen(CancellationToken cancellationToken);
        Task Shutdown(CancellationToken cancellationToken);
    }
}
