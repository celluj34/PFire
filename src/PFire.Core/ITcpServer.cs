using System;
using System.Threading;
using System.Threading.Tasks;
using PFire.Core.Protocol.Messages;
using PFire.Core.Session;

namespace PFire.Core
{
    internal interface ITcpServer
    {
        delegate Task OnConnectionHandler(IXFireClient sessionContext);

        delegate Task OnDisconnectionHandler(IXFireClient sessionContext);

        delegate Task OnReceiveHandler(IXFireClient sessionContext, IMessage message);

        event OnReceiveHandler OnReceive;
        event OnConnectionHandler OnConnection;
        event OnDisconnectionHandler OnDisconnection;
        Task Listen(CancellationToken cancellationToken);
        Task Shutdown(CancellationToken cancellationToken);
    }
}
