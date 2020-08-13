using System.Threading;
using System.Threading.Tasks;
using Laso.Scheduling.Core.Experiments.Queries;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.Scheduling.UnitTests.Core.Experiments.Queries
{
    public class GetPartnerExperimentConfigurationHandlerTests
    {
        [Fact]
        public async Task When_QuarterSport_Should_enable_experiments()
        {
            // Arrange
            var handler = new GetPartnerExperimentConfigurationHandler();
            var input = new GetPartnerExperimentConfigurationQuery("6c34c5bb-b083-4e62-a83e-cb0532754809");

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
            response.Result.ExperimentsEnabled.ShouldBeTrue();
        }

        [Fact]
        public async Task When_not_QuarterSport_Should_disable_experiments()
        {
            // Arrange
            var handler = new GetPartnerExperimentConfigurationHandler();
            var input = new GetPartnerExperimentConfigurationQuery("not quarterspot");

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
            response.Result.ExperimentsEnabled.ShouldBeFalse();
        }
    }
}
