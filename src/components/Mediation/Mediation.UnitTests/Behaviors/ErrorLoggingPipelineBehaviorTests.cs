using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Mediation.Behaviors;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.Mediation.UnitTests.Behaviors
{
    public class ErrorLoggingPipelineBehaviorTests
    {
        [Fact]
        public async Task When_succeeded_Should_not_log()
        {
            // Arrange
            var logger = new InMemoryLogger<ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Succeeded(new TestResult());

            // Act
            var response = await behavior.Handle(input, CancellationToken.None, () => Task.FromResult(result));

            // Assert
            response.Success.ShouldBeTrue();
            logger.LogMessages.ShouldBeEmpty();
        }

        [Fact]
        public async Task When_exception_thrown_Should_log_error()
        {
            // Arrange
            var logger = new InMemoryLogger<ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await behavior.Handle(input, CancellationToken.None, () => throw new Exception("kaboom")));

            // Assert
            exception.Message.ShouldBe("kaboom");
            var logMessage = logger.LogMessages.Single();
            logMessage.Exception.ShouldBeSameAs(exception);
            logMessage.LogLevel.ShouldBe(LogLevel.Error);
        }

        [Fact]
        public async Task When_response_failed_with_message_Should_log_warning()
        {
            // Arrange
            var logger = new InMemoryLogger<ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Failed<TestResult>("key", "message");

            // Act
            var response = await behavior.Handle(input, CancellationToken.None, () => Task.FromResult(result));

            // Assert
            response.Success.ShouldBeFalse();
            var logMessage = logger.LogMessages.Single();
            logMessage.Exception.ShouldBeNull();
            logMessage.LogLevel.ShouldBe(LogLevel.Warning);
        }

        [Fact]
        public async Task When_response_failed_with_exception_Should_log_error()
        {
            // Arrange
            var logger = new InMemoryLogger<ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new ErrorLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Failed<TestResult>(new Exception("kaboom"));

            // Act
            var response = await behavior.Handle(input, CancellationToken.None, () => Task.FromResult(result));

            // Assert
            response.Success.ShouldBeFalse();
            var logMessage = logger.LogMessages.Single();
            logMessage.Exception.Message.ShouldBe("kaboom");
            logMessage.LogLevel.ShouldBe(LogLevel.Error);
        }
    }
}
