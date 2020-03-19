using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Infrastructure.IntegrationEvents;
using Shouldly;
using Xunit;

namespace Laso.Identity.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusEventPublisherAndListenerTests
    {
        [Fact]
        public async Task Should_publish_and_receive_event()
        {
            var id = Guid.NewGuid().ToString("D");

            using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>();

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

                await eventPublisher.Publish(new TestEvent { Id = id });

                var @event = await subscription.WaitForMessage();
                @event.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_publish_and_receive_event_on_filtered_subscription()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEnvelopedEvent<TestBody1>>(x => x.Type == nameof(TestBody1));

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

                await eventPublisher.Publish(new TestEnvelopedEvent<TestBody1> { Body = new TestBody1 { Id = id1 } });
                await eventPublisher.Publish(new TestEnvelopedEvent<TestBody2> { Body = new TestBody2 { Id = id2 } });

                var event1 = await subscription.WaitForMessage();
                event1.Body.Id.ShouldBe(id1);

                var event2 = await subscription.WaitForMessage();
                event2.ShouldBeNull();
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }

        private class TestEnvelopedEvent<TBody> : IEnvelopedIntegrationEvent
        {
            public string Type => Body.GetType().Name;
            public TBody Body { get; set; }

            [JsonIgnore]
            public (string Name, object Value) Discriminator => (nameof(Type), Type);
        }

        private class TestBody1
        {
            public string Id { get; set; }
        }

        private class TestBody2
        {
            public string Id { get; set; }
        }
    }
}
