using System.Threading.Tasks;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly IMessageBuilder _messageBuilder;

        public AzureServiceBusEventPublisher(AzureServiceBusTopicProvider topicProvider, IMessageBuilder messageBuilder)
        {
            _topicProvider = topicProvider;
            _messageBuilder = messageBuilder;
        }

        public async Task Publish<T>(T @event, string topicName = null) where T : IIntegrationEvent
        {
            topicName ??= @event.GetType().Name;

            var client = await _topicProvider.GetTopicClient(topicName);

            var message = _messageBuilder.Build(@event, topicName);

            await client.SendAsync(message);
        }
    }
}
