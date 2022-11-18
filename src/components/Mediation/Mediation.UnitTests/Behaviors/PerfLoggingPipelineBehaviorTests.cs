using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Behaviors;
using Infrastructure.Mediation.Query;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Infrastructure.Mediation.UnitTests.Behaviors
{
    public class PerfLoggingPipelineBehaviorTests
    {
        [Fact]
        public async Task When_succeeded_Should_log_stats()
        {
            // Arrange
            var logger = new InMemoryLogger<PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Succeeded(new TestResult());

            // Act
            var response = await behavior.Handle(input, () => Task.FromResult(result), CancellationToken.None);

            // Assert
            response.Success.ShouldBeTrue();
            AssertLogs(logger);
        }

        private static void AssertLogs(InMemoryLogger<PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>> logger)
        {
            logger.LogMessages.Count.ShouldBe(2);
            logger.LogMessages.Single(m => m.LogLevel == LogLevel.Debug).Message.ShouldContain("Started");
            logger.LogMessages.Single(m => m.LogLevel == LogLevel.Information).Message.ShouldContain("Completed");
        }

        [Fact]
        public async Task When_exception_thrown_Should_log_stats()
        {
            // Arrange
            var logger = new InMemoryLogger<PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => await behavior.Handle(input, () => throw new Exception("kaboom"), CancellationToken.None));

            // Assert
            exception.Message.ShouldBe("kaboom");
            AssertLogs(logger);
        }

        [Fact]
        public async Task When_response_failed_with_message_Should_log_stats()
        {
            // Arrange
            var logger = new InMemoryLogger<PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Failed<TestResult>("key", "message");

            // Act
            var response = await behavior.Handle(input, () => Task.FromResult(result), CancellationToken.None);

            // Assert
            response.Success.ShouldBeFalse();
            AssertLogs(logger);
        }

        [Fact]
        public async Task When_response_failed_with_exception_Should_log_stats()
        {
            // Arrange
            var logger = new InMemoryLogger<PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>>();
            var behavior = new PerfLoggingPipelineBehavior<TestQuery, QueryResponse<TestResult>>(logger);
            var input = new TestQuery();
            var result = QueryResponse.Failed<TestResult>(new Exception("kaboom"));

            // Act
            var response = await behavior.Handle(input, () => Task.FromResult(result), CancellationToken.None);

            // Assert
            response.Success.ShouldBeFalse();
            AssertLogs(logger);
        }
    }
}
