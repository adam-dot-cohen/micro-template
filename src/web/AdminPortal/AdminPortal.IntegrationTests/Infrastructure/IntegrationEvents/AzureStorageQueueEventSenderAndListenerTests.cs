using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.IntegrationEvents;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueEventSenderAndListenerTests
    {
        [Fact]
        public async Task Should_publish_and_receive_event()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var receiver = await queueProvider.AddReceiver<TestEvent>();

                var eventPublisher = new AzureStorageQueueEventSender(queueProvider);

                await eventPublisher.Send(new TestEvent { Id = id });

                var @event = await receiver.WaitForMessage();
                @event.DeserializedMessage.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_dead_letter_message_after_three_failures()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var subscription = await queueProvider.AddReceiver<TestEvent>(onReceive: x => throw new Exception());

                var eventPublisher = new AzureStorageQueueEventSender(queueProvider);

                await eventPublisher.Send(new TestEvent { Id = id });

                var @event = await subscription.WaitForDeadLetterMessage();
                @event.Id.ShouldBe(id);

                var messages = subscription.GetAllMessageResults();
                messages.Count.ShouldBe(3);
                messages.Count(x => x.Exception != null).ShouldBe(3);
                messages.Count(x => x.WasAbandoned).ShouldBe(2);
                messages.Count(x => x.WasDeadLettered).ShouldBe(1);
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }
    }
}
