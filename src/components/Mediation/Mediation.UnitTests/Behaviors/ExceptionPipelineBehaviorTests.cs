using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Behaviors;
using Infrastructure.Mediation.Query;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Infrastructure.Mediation.UnitTests.Behaviors
{
    public class ExceptionPipelineBehaviorTests
    {
        [Fact]
        public async Task When_no_exception_Should_succeed()
        {
            // Arrange
            var behavior = new ExceptionPipelineBehavior<TestQuery, QueryResponse<TestResult>>();
            var input = new TestQuery();
            var result = QueryResponse.Succeeded(new TestResult());

            // Act
            var response = await behavior.Handle(input, () => Task.FromResult(result), CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task When_exception_Should_fail()
        {
            // Arrange
            var behavior = new ExceptionPipelineBehavior<TestQuery, QueryResponse<TestResult>>();
            var input = new TestQuery();

            // Act
            var response = await behavior.Handle(input, () => throw new Exception("kaboom"), CancellationToken.None);

            // Assert
            response.Success.ShouldBeFalse();
            response.Exception?.Message.ShouldBe("kaboom");
        }
    }
}
