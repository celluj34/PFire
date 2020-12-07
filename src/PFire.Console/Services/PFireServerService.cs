using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PFire.Core;

namespace PFire.Console.Services
{
    internal class PFireServerService : BackgroundService
    {
        private readonly IPFireServer _pfServer;

        public PFireServerService(IPFireServer pFireServer)
        {
            _pfServer = pFireServer;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _pfServer.Execute(cancellationToken);
        }
    }
}
