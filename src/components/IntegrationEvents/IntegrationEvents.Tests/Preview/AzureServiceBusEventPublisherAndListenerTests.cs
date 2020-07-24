using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.IO.Serialization.Newtonsoft;
using Shouldly;
using Xunit;
using AzureServiceBusEventPublisher = Laso.IntegrationEvents.AzureServiceBus.Preview.AzureServiceBusEventPublisher;

namespace Laso.IntegrationEvents.Tests.Preview
{
    public class AzureServiceBusEventPublisherAndListenerTests
    {
        [Fact]
        public async Task Should_publish_and_receive_event()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>();

                (await subscription.GetFilter()).ShouldBe("1=1");

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider, new NewtonsoftSerializer());

                await eventPublisher.Publish(new TestEvent { Id = id });

                var @event = await subscription.WaitForMessage();
                @event.Event.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_dead_letter_message_after_three_failures()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>(onReceive: x => throw new Exception());

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider, new NewtonsoftSerializer());

                await eventPublisher.Publish(new TestEvent { Id = id });

                var @event = await subscription.WaitForDeadLetterMessage();
                @event.Id.ShouldBe(id);

                var messages = new[]
                {
                    await subscription.WaitForMessage(),
                    await subscription.WaitForMessage(),
                    await subscription.WaitForMessage()
                };
                messages.Count(x => x.Exception != null).ShouldBe(3);
                messages.Count(x => x.WasAbandoned).ShouldBe(2);
                messages.Count(x => x.WasDeadLettered).ShouldBe(1);
            }
        }

        [Fact]
        public async Task Should_update_filter_when_changed()
        {
            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>(subscriptionName: "TestSubscription");

                (await subscription.GetFilter()).ShouldBe("1=1");

                subscription = await topicProvider.AddSubscription<TestEvent>(subscriptionName: "TestSubscription", sqlFilter: "1=0");

                (await subscription.GetFilter()).ShouldBe("1=0");
            }
        }

        [Fact]
        public async Task Should_not_update_filter_when_not_changed()
        {
            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>(subscriptionName: "TestSubscription");

                (await subscription.GetFilter()).ShouldBe("1=1");

                subscription = await topicProvider.AddSubscription<TestEvent>(subscriptionName: "TestSubscription");

                (await subscription.GetFilter()).ShouldBe("1=1");
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }
    }
}
