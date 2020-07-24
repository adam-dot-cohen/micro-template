using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Laso.IO.Serialization;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
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
            var client = _topicProvider.GetSender(@event.GetType());

            var bytes = _serializer.SerializeToUtf8Bytes(@event);

            var message = new ServiceBusMessage(bytes);

            await client.SendMessageAsync(message);
        }
    }
}
