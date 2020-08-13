using System.Threading;
using System.Threading.Tasks;
using Laso.Mediation.Behaviors;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.Mediation.UnitTests.Behaviors
{
    public class ValidationPipelineBehaviorTests
    {
        [Fact]
        public async Task When_valid_Should_succeed()
        {
            // Arrange
            var behavior = new ValidationPipelineBehavior<TestQuery, QueryResponse<TestResult>>();
            var input = new TestQuery();
            var result = QueryResponse.Succeeded(new TestResult());

            // Act
            var response = await behavior.Handle(input, CancellationToken.None, () => Task.FromResult(result));

            // Assert
            response.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task When_invalid_Should_fail()
        {
            // Arrange
            var behavior = new ValidationPipelineBehavior<TestQuery, QueryResponse<TestResult>>();
            var input = new TestQuery(new ValidationMessage("key", "bad"));
            var result = QueryResponse.Succeeded(new TestResult());

            // Act
            var response = await behavior.Handle(input, CancellationToken.None, () => Task.FromResult(result));

            // Assert
            response.Success.ShouldBeFalse();
            response.ValidationMessages[0].Key.ShouldBe("key");
            response.ValidationMessages[0].Message.ShouldBe("bad");
        }
    }

}
