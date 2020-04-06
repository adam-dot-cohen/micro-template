using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

//// ReSharper disable StyleCop.SA1402

namespace Laso.AdminPortal.UnitTests.Infrastructure
{
    public abstract class MediatorTests
    {
        private readonly IMediator _mediator;

        protected MediatorTests()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();
            services.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();
            services.AddTransient<ICommandHandler<TestWithResultCommand, string>, TestWithResultCommandHandler>();

            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
            
            _mediator = new Mediator(serviceProvider, new NullLogger<Mediator>());
        }

        public class WhenQueryInvoked : MediatorTests
        {
            private readonly Guid _testId;
            private readonly QueryResponse<TestResult> _response;

            public WhenQueryInvoked()
            {
                // Arrange
                _testId = Guid.NewGuid();
                var query = new TestQuery { Id = _testId };

                // Act
                _response = _mediator.Query(query, CancellationToken.None).Result;
            }

            [Fact]
            public void QueryHandler_Should_Succeed()
            {
                _response.ShouldNotBeNull();
                _response.Success.ShouldBeTrue();
                _response.Result.TestId.ShouldBe(_testId);
            }
        }

        public class WhenCommandInvoked : MediatorTests
        {
            private readonly CommandResponse _response;

            public WhenCommandInvoked()
            {
                var command = new TestCommand();

                // Act
                _response = _mediator.Command(command, CancellationToken.None).Result;
            }

            [Fact]
            public void CommandHandler_Should_Succeed()
            {
                _response.ShouldNotBeNull();
                _response.Success.ShouldBeTrue();
            }
        }

        public class WhenCommandWithResultInvoked : MediatorTests
        {
            private readonly CommandResponse<string> _response;

            public WhenCommandWithResultInvoked()
            {
                var command = new TestWithResultCommand();

                // Act
                _response = _mediator.Command(command, CancellationToken.None).Result;
            }

            [Fact]
            public void CommandHandler_Should_Succeed()
            {
                _response.ShouldNotBeNull();
                _response.Success.ShouldBeTrue();
                _response.Result.ShouldNotBeNullOrEmpty();
            }
        }
    }

    public class TestQuery : IQuery<TestResult>
    {
        public Guid Id { get; set; }
    }

    public class TestResult
    {
        public Guid TestId { get; set; }
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, TestResult>
    {
        public Task<QueryResponse<TestResult>> Handle(TestQuery query, CancellationToken cancellationToken)
        {
            var result = new TestResult
            {
                TestId = query.Id
            };

            return Task.FromResult(QueryResponse.Succeeded(result));
        }
    }

    public class TestCommand : ICommand
    {
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<CommandResponse> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(CommandResponse.Succeeded());
        }
    }

    public class TestWithResultCommand : ICommand<string>
    {
    }

    public class TestWithResultCommandHandler : ICommandHandler<TestWithResultCommand, string>
    {
        public Task<CommandResponse<string>> Handle(TestWithResultCommand command, CancellationToken cancellationToken)
        {
            var result = Guid.NewGuid().ToString();
            return Task.FromResult(CommandResponse.Succeeded(result));
        }
    }
}
