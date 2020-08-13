using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shouldly;
using Xunit;

namespace Laso.Mediation.UnitTests.Configuration
{
    public class LamarCommandHandlerTests
    {
        [Fact]
        public async Task Should_register_command_handlers()
        {
            var repository = new Repository();
            var container = new TestContainer(repository);

            var instance = container.GetInstance<IRequestHandler<TestCommand, CommandResponse>>();

            instance.ShouldBeOfType<TestHandler>();

            var command = new TestCommand();

            await container.GetInstance<IMediator>().Send(command);

            repository[typeof(TestHandler)].ShouldBeSameAs(command);
        }

        public class TestCommand : ICommand { }

        public class TestHandler : CommandHandler<TestCommand>
        {
            private readonly Repository _repository;

            public TestHandler(Repository repository)
            {
                _repository = repository;
            }

            public override Task<CommandResponse> Handle(TestCommand request, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler), request);

                return Task.FromResult(Succeeded());
            }
        }
    }
}
