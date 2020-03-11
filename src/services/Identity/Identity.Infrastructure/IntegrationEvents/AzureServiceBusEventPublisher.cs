using System.Text.Json;
using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;
using Microsoft.Azure.ServiceBus;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;

        public AzureServiceBusEventPublisher(AzureServiceBusTopicProvider topicProvider)
        {
            _topicProvider = topicProvider;
        }

        public async Task Publish(object @event)
        {
            var client = await _topicProvider.GetTopicClient(@event.GetType());

            var bytes = JsonSerializer.SerializeToUtf8Bytes(@event);

            await client.SendAsync(new Message(bytes));
        }
    }
}
