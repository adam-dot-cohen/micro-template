using System.Threading.Tasks;
using Laso.IO.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly ISerializer _serializer;

        public AzureServiceBusEventPublisher(AzureServiceBusTopicProvider topicProvider, ISerializer serializer)
        {
            _topicProvider = topicProvider;
            _serializer = serializer;
        }

        public async Task Publish<T>(T @event) where T : IIntegrationEvent
        {
            var client = await _topicProvider.GetTopicClient(@event.GetType());

            var bytes = _serializer.SerializeToUtf8Bytes(@event);

            var message = new Message(bytes);

            await client.SendAsync(message);
        }
    }
}
