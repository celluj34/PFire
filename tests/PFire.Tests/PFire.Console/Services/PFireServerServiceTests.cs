using System.Threading;
using System.Threading.Tasks;
using Moq;
using PFire.Console.Services;
using PFire.Core.Services;
using Xunit;

namespace PFire.Tests.PFire.Console.Services
{
    public class PFireServerServiceTests : BaseTest
    {
        [Fact]
        public async Task StartAsync_Calls_Start()
        {
            //arrange
            var cancellationToken = new CancellationToken();
            var pFireServerMock = _autoMoqer.GetMock<IPFireServer>();

            var service = _autoMoqer.CreateInstance<PFireServerService>();

            //act
            await service.StartAsync(cancellationToken);

            //assert
            pFireServerMock.Verify(x => x.Start(cancellationToken), Times.Once);
            pFireServerMock.Verify(x => x.Stop(cancellationToken), Times.Never);
        }

        [Fact]
        public async Task StopAsync_Calls_Stop()
        {
            //arrange
            var cancellationToken = new CancellationToken();
            var pFireServerMock = _autoMoqer.GetMock<IPFireServer>();

            var service = _autoMoqer.CreateInstance<PFireServerService>();

            //act
            await service.StopAsync(cancellationToken);

            //assert
            pFireServerMock.Verify(x => x.Start(cancellationToken), Times.Never);
            pFireServerMock.Verify(x => x.Stop(cancellationToken), Times.Once);
        }
    }
}
