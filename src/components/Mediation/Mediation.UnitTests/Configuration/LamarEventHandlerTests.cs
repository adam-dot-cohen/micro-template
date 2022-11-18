using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Behaviors;
using Infrastructure.Mediation.Event;
using MediatR;
using Shouldly;
using Xunit;

namespace Infrastructure.Mediation.UnitTests.Configuration
{
    public class LamarEventHandlerTests
    {
        [Fact]
        public async Task Should_register_event_pipelines()
        {
            var repository = new Repository();
            var container = new TestContainer(repository);

            var instances = container.GetAllInstances<INotificationHandler<TestEvent1>>();

            instances.Count.ShouldBe(2);
            instances.ShouldContain(x => x.GetType() == typeof(EventPipeline<TestEvent1, TestHandler1>));
            instances.ShouldContain(x => x.GetType() == typeof(EventPipeline<TestEvent1, TestHandler2>));

            var @event = new TestEvent1();

            await container.GetInstance<IMediator>().Publish(@event);

            repository[typeof(TestHandler1)].ShouldBeSameAs(@event);
            repository[typeof(TestHandler2)].ShouldBeSameAs(@event);
        }

        [Fact]
        public async Task Should_register_all_event_pipelines_for_a_given_type()
        {
            var repository = new Repository();
            var container = new TestContainer(repository);

            var instances = container.GetAllInstances<INotificationHandler<TestEvent2>>();

            instances.Count.ShouldBe(1);
            instances.ShouldContain(x => x.GetType() == typeof(EventPipeline<TestEvent2, TestHandler2>));

            var @event = new TestEvent2();

            await container.GetInstance<IMediator>().Publish(@event);

            repository[typeof(TestHandler2)].ShouldBeSameAs(@event);
        }

        public class TestEvent1 : IEvent { }
        public class TestEvent2 : IEvent { }

        public class TestHandler1 : EventHandler<TestEvent1>
        {
            private readonly Repository _repository;

            public TestHandler1(Repository repository)
            {
                _repository = repository;
            }

            public override Task<EventResponse> Handle(TestEvent1 notification, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler1), notification);

                return Task.FromResult(Succeeded());
            }
        }

        public class TestHandler2 : IEventHandler<TestEvent1>, IEventHandler<TestEvent2>
        {
            private readonly Repository _repository;

            public TestHandler2(Repository repository)
            {
                _repository = repository;
            }

            public Task<EventResponse> Handle(TestEvent1 notification, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler2), notification);

                return Task.FromResult(EventResponse.Succeeded());
            }

            public Task<EventResponse> Handle(TestEvent2 notification, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler2), notification);

                return Task.FromResult(EventResponse.Succeeded());
            }
        }
    }
}
