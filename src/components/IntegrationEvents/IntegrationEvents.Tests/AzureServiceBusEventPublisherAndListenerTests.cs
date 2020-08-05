using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IntegrationEvents.Tests.Extensions;
using Laso.IO.Serialization.Newtonsoft;
using Shouldly;
using Xunit;

namespace Laso.IntegrationEvents.Tests
{
    public class AzureServiceBusEventPublisherAndListenerTests
    {
        [Fact]
        public async Task Should_publish_and_receive_event()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var topic = topicProvider.GetTopic<TestEvent>();

                var subscription = await topic.AddSubscription();

                (await subscription.GetFilter()).ShouldBe("1=1");

                var eventPublisher = topic.GetPublisher();

                await eventPublisher.Publish(new TestEvent { Id = id });

                var @event = await subscription.WaitForMessage();
                @event.Event.Id.ShouldBe(id);
            }
        }
        [Fact]
        public async Task Should_publish_and_receive_cloud_event()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var topic = topicProvider.GetTopic<TestEvent>(isCloudEvent: true);

                var subscription = await topic.AddSubscription();

                (await subscription.GetFilter()).ShouldBe("1=1");

                var eventPublisher = topic.GetPublisher();

                var activity = new Activity("test");
                activity.SetTraceParent();
                activity.TraceStateString = "laso=test";
                activity.Start();

                await eventPublisher.Publish(new TestEvent { Id = id });

                activity.Stop();

                var @event = await subscription.WaitForMessage();
                @event.Event.Id.ShouldBe(id);
                @event.Context.TraceParent.ShouldBe(activity.GetTraceParent());
                @event.Context.TraceState.ShouldBe(activity.TraceStateString);
            }
        }

        [Fact]
        public async Task Should_publish_and_receive_event_with_topic_specified()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var topic = topicProvider.GetTopic<TestEvent>("test");

                var subscription = await topic.AddSubscription();

                (await subscription.GetFilter()).ShouldBe("1=1");

                var eventPublisher = topic.GetPublisher();

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
                var topic = topicProvider.GetTopic<TestEvent>();

                var subscription = await topic.AddSubscription(onReceive: x => throw new Exception());

                var eventPublisher = topic.GetPublisher();

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
                var topic = topicProvider.GetTopic<TestEvent>();

                var subscription = await topic.AddSubscription(subscriptionName: "TestSubscription");

                (await subscription.GetFilter()).ShouldBe("1=1");

                subscription = await topic.AddSubscription(subscriptionName: "TestSubscription", sqlFilter: "1=0");

                (await subscription.GetFilter()).ShouldBe("1=0");
            }
        }

        [Fact]
        public async Task Should_not_update_filter_when_not_changed()
        {
            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var topic = topicProvider.GetTopic<TestEvent>();

                var subscription = await topic.AddSubscription();

                (await subscription.GetFilter()).ShouldBe("1=1");

                subscription = await topic.AddSubscription();

                (await subscription.GetFilter()).ShouldBe("1=1");
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }
    }
}
