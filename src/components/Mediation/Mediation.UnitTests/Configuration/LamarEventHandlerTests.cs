using System.Threading;
using System.Threading.Tasks;
using Laso.Mediation.Behaviors;
using MediatR;
using Shouldly;
using Xunit;

namespace Laso.Mediation.UnitTests.Configuration
{
    public class LamarEventHandlerTests
    {
        [Fact]
        public async Task Should_register_event_pipelines()
        {
            var repository = new Repository();
            var container = new TestContainer(repository);

            var instances = container.GetAllInstances<INotificationHandler<TestEvent>>();

            instances.Count.ShouldBe(2);
            instances.ShouldContain(x => x.GetType() == typeof(EventPipeline<TestEvent, TestHandler1>));
            instances.ShouldContain(x => x.GetType() == typeof(EventPipeline<TestEvent, TestHandler2>));

            var @event = new TestEvent();

            await container.GetInstance<IMediator>().Publish(@event);

            repository[typeof(TestHandler1)].ShouldBeSameAs(@event);
            repository[typeof(TestHandler2)].ShouldBeSameAs(@event);
        }

        public class TestEvent : IEvent { }

        public class TestHandler1 : EventHandler<TestEvent>
        {
            private readonly Repository _repository;

            public TestHandler1(Repository repository)
            {
                _repository = repository;
            }

            public override Task<EventResponse> Handle(TestEvent notification, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler1), notification);

                return Task.FromResult(Succeeded());
            }
        }

        public class TestHandler2 : EventHandler<TestEvent>
        {
            private readonly Repository _repository;

            public TestHandler2(Repository repository)
            {
                _repository = repository;
            }

            public override Task<EventResponse> Handle(TestEvent notification, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler2), notification);

                return Task.FromResult(Succeeded());
            }
        }
    }
}
