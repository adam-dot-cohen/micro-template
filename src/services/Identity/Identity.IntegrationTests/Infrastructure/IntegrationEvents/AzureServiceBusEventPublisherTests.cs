using System;
using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Infrastructure.IntegrationEvents;
using Shouldly;
using Xunit;

namespace Laso.Identity.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusEventPublisherTests
    {
        [Fact]
        public async Task Should_publish_message()
        {
            var id = Guid.NewGuid().ToString("D");

            using (var topicProvider = new TempAzureServiceBusTopicProvider())
            {
                var subscription = await topicProvider.AddSubscription<TestEvent>();

                var eventPublisher = new AzureServiceBusEventPublisher(topicProvider);

                await eventPublisher.Publish(new TestEvent { Id = id });

                (await subscription.WaitForMessage()).Id.ShouldBe(id);
            }
        }

        private class TestEvent : IIntegrationEvent
        {
            public string Id { get; set; }
        }
    }
}
