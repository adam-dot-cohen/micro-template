using System.Text.Json;
using System.Threading.Tasks;
using Laso.Identity.Core.Messaging;
using Microsoft.Azure.ServiceBus;

namespace Laso.Identity.Infrastructure.Eventing
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureTopicProvider _topicProvider;

        public AzureServiceBusEventPublisher(AzureTopicProvider topicProvider)
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
