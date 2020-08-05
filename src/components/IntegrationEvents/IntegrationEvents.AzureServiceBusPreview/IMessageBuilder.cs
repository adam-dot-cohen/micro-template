using Azure.Messaging.ServiceBus;
using Laso.IO.Serialization;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public interface IMessageBuilder
    {
        ServiceBusMessage Build<T>(T @event, string topicName) where T : IIntegrationEvent;
    }

    public class DefaultMessageBuilder : IMessageBuilder
    {
        private readonly ISerializer _serializer;

        public DefaultMessageBuilder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public ServiceBusMessage Build<T>(T @event, string topicName) where T : IIntegrationEvent
        {
            var bytes = _serializer.SerializeToUtf8Bytes(@event);

            return new ServiceBusMessage(bytes);
        }
    }
}