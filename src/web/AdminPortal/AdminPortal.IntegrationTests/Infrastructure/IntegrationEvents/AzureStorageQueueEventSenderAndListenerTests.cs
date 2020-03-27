using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
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

                var eventSender = queueProvider.GetSender();

                await eventSender.Send(new TestEvent { Id = id });

                var @event = await receiver.WaitForMessage();
                @event.Event.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_dead_letter_message_after_three_failures()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var listener = await queueProvider.AddReceiver<TestEvent>(onReceive: x => throw new Exception());

                var eventSender = queueProvider.GetSender();

                await eventSender.Send(new TestEvent { Id = id });

                var deadLetterQueueEvent = await listener.WaitForDeadLetterMessage();

                deadLetterQueueEvent.DeadLetterEvent.OriginatingQueue.ShouldStartWith(nameof(TestEvent), Case.Insensitive);
                deadLetterQueueEvent.DeadLetterEvent.Exception.ShouldNotBeNullOrWhiteSpace();
                deadLetterQueueEvent.Event.Id.ShouldBe(id);

                var messages = new[]
                {
                    await listener.WaitForMessage(),
                    await listener.WaitForMessage(),
                    await listener.WaitForMessage()
                };
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
