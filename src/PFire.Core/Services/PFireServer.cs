using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PFire.Core.Messages.Outbound;

namespace PFire.Core.Services
{
    public interface IPFireServer
    {
        Task Start(CancellationToken cancellationToken);
        Task Stop(CancellationToken cancellationToken);
    }

    internal sealed class PFireServer : IPFireServer
    {
        private readonly IXFireClientManager _clientManager;
        private readonly IPFireDatabase _database;
        private readonly IXFireTcpListener _listener;
        private readonly ILogger<PFireServer> _logger;
        private readonly IXFireClientProvider _xFireClientProvider;
        private bool _running;

        public PFireServer(IPFireDatabase pFireDatabase,
                           IXFireClientManager xFireClientManager,
                           IXFireClientProvider xFireClientProvider,
                           IXFireTcpListener listener,
                           ILogger<PFireServer> logger)
        {
            _database = pFireDatabase;
            _clientManager = xFireClientManager;
            _xFireClientProvider = xFireClientProvider;
            _listener = listener;
            _logger = logger;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            _running = true;
            _listener.Start();
            _logger.LogInformation($"PFire Server listening on {_listener.LocalEndpoint}");
            Task.Run(() => Accept().ConfigureAwait(false), cancellationToken);

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _running = false;

            return Task.CompletedTask;
        }

        private async Task Accept()
        {
            while (_running)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                var newXFireClient = _xFireClientProvider.GetClient(tcpClient, OnDisconnection);

                _clientManager.AddSession(newXFireClient);
            }
        }

        private async Task OnDisconnection(IXFireClient disconnectedClient)
        {
            // we have to remove the session first 
            // because of the friends of this user processing
            _clientManager.RemoveSession(disconnectedClient);

            var friends = await _database.QueryFriends(disconnectedClient.User);
            foreach (var friend in friends)
            {
                var friendClient = _clientManager.GetSession(friend);
                friendClient?.SendAndProcessMessage(new FriendsSessionAssign(friend));
            }

            disconnectedClient.Dispose();
        }
    }
}
