using System.Threading.Tasks;
using Laso.IO.Serialization;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class AzureStorageQueueMessageSender : IMessageSender
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly ISerializer _serializer;

        public AzureStorageQueueMessageSender(AzureStorageQueueProvider queueProvider, ISerializer serializer)
        {
            _queueProvider = queueProvider;
            _serializer = serializer;
        }

        public async Task Send<T>(T message) where T : IIntegrationMessage
        {
            var client = await _queueProvider.GetQueue(message.GetType());

            var text = _serializer.Serialize(message);

            await client.SendMessageAsync(text);
        }
    }
}
