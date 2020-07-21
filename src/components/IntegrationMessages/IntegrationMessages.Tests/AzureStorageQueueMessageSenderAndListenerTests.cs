using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Laso.IntegrationMessages.Tests
{
    public class AzureStorageQueueMessageSenderAndListenerTests
    {
        [Fact]
        public async Task Should_publish_and_receive_message()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var receiver = await queueProvider.AddReceiver<TestMessage>();

                var messageSender = queueProvider.GetSender();

                await messageSender.Send(new TestMessage { Id = id });

                var message = await receiver.WaitForMessage();
                message.Message.Id.ShouldBe(id);
            }
        }

        [Fact]
        public async Task Should_dead_letter_message_after_three_failures()
        {
            var id = Guid.NewGuid().ToString("D");

            await using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var listener = await queueProvider.AddReceiver<TestMessage>(onReceive: x => throw new Exception());

                var messageSender = queueProvider.GetSender();

                await messageSender.Send(new TestMessage { Id = id });

                var deadLetterQueueMessage = await listener.WaitForDeadLetterMessage();

                deadLetterQueueMessage.DeadLetterMessage.OriginatingQueue.ShouldStartWith(nameof(TestMessage), Case.Insensitive);
                deadLetterQueueMessage.DeadLetterMessage.Exception.ShouldNotBeNullOrWhiteSpace();
                deadLetterQueueMessage.Message.Id.ShouldBe(id);

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

        private class TestMessage : IIntegrationMessage
        {
            public string Id { get; set; }
        }
    }
}
