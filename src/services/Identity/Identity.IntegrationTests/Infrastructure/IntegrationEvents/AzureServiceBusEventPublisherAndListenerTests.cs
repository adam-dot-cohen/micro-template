﻿using System;
using System.Linq;
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

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>();

                (await subscription.GetFilter()).ShouldBe("1=1");

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

                await eventPublisher.Publish(new TestEvent { Id = id });

                var @event = await subscription.WaitForMessage();
                @event.Event.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_publish_and_receive_event_on_filtered_subscription()
        {
            var id1 = Guid.NewGuid().ToString("D");
            var id2 = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEnvelopedEvent<TestBody2>>(sqlFilter: "EventType = 'TestBody2'");

                (await subscription.GetFilter()).ShouldBe("EventType = 'TestBody2'");

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

                await eventPublisher.Publish(new TestEnvelopedEvent<TestBody1> { Body = new TestBody1 { Id = id1, Asdf = "Asdf" } });
                await eventPublisher.Publish(new TestEnvelopedEvent<TestBody2> { Body = new TestBody2 { Id = id2, Fdsa = "Fdsa" } });

                var @event = await subscription.WaitForMessage();
                @event.Event.Body.Id.ShouldBe(id2);
                @event.Event.Body.Fdsa.ShouldBe("Fdsa");
            }
        }

        [Fact]
        public async Task Should_dead_letter_message_after_three_failures()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>(onReceive: x => throw new Exception());

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

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

                subscription = await topicProvider.AddSubscription<TestEvent>(subscriptionName: "TestSubscription", filter: x => false);

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

        private class TestEnvelopedEvent<TBody> : IIntegrationEvent
        {
            public TBody Body { get; set; }
        }

        private abstract class TestBody
        {
            [EnvelopeProperty(Name = "EventType")]
            public string Type => GetType().Name;

            public string Id { get; set; }
        }

        private class TestBody1 : TestBody
        {
            public string Asdf { get; set; }
        }

        private class TestBody2 : TestBody
        {
            public string Fdsa { get; set; }
        }
    }
}
