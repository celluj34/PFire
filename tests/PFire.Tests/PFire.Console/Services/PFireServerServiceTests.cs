using System.Threading;
using System.Threading.Tasks;
using Moq;
using PFire.Console.Services;
using PFire.Core;
using Xunit;

namespace PFire.Tests.PFire.Console.Services
{
    public class PFireServerServiceTests : BaseTest
    {
        [Fact]
        public async Task StartAsync_Calls_Execute()
        {
            //arrange
            var cancellationToken = new CancellationToken();

            var pFireServerMock = _autoMoqer.GetMock<IPFireServer>();

            var service = _autoMoqer.CreateInstance<PFireServerService>();

            //act
            await service.StartAsync(cancellationToken);

            //assert
            pFireServerMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
