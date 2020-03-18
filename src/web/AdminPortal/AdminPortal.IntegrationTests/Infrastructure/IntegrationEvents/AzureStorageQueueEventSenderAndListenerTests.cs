using System;
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

            using (var queueProvider = new TempAzureStorageQueueProvider())
            {
                var subscription = await queueProvider.AddSubscription<TestEvent>();

                var eventPublisher = new AzureStorageQueueEventSender(queueProvider);

                await eventPublisher.Send(new TestEvent { Id = id });

                var @event = await subscription.WaitForMessage();
                @event.Id.ShouldBe(id);
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }
    }
}
