using System.Text.Json;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Microsoft.Azure.ServiceBus;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;

        public AzureServiceBusEventPublisher(AzureServiceBusTopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public async Task Publish<T>(T @event) where T : IIntegrationEvent
        {
            var client = await _topicProvider.GetTopicClient(@event.GetType());

            var bytes = JsonSerializer.SerializeToUtf8Bytes(@event);

            await client.SendAsync(new Message(bytes));
        }
    }
}
