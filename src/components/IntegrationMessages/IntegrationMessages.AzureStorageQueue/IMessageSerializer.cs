using Laso.IO.Serialization;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public interface IMessageSerializer
    {
        string Serialize<T>(T message) where T : IIntegrationMessage;
    }

    public class DefaultMessageSerializer : IMessageSerializer
    {
        private readonly ISerializer _serializer;

        public DefaultMessageSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public string Serialize<T>(T message) where T : IIntegrationMessage
        {
            return _serializer.Serialize(message);
        }
    }
}