using Laso.IO.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public interface IMessageBuilder
    {
        Message Build<T>(T @event, string topicName) where T : IIntegrationEvent;
    }

    public class DefaultMessageBuilder : IMessageBuilder
    {
        private readonly ISerializer _serializer;

        public DefaultMessageBuilder(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public Message Build<T>(T @event, string topicName) where T : IIntegrationEvent
        {
            var bytes = _serializer.SerializeToUtf8Bytes(@event);

            return new Message(bytes);
        }
    }
}