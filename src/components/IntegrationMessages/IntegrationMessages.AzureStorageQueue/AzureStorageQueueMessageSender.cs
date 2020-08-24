using System.Threading.Tasks;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class AzureStorageQueueMessageSender : IMessageSender
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly IMessageSerializer _messageSerializer;

        public AzureStorageQueueMessageSender(AzureStorageQueueProvider queueProvider, IMessageSerializer messageSerializer)
        {
            _queueProvider = queueProvider;
            _messageSerializer = messageSerializer;
        }

        public async Task Send<T>(T message) where T : IIntegrationMessage
        {
            var client = await _queueProvider.GetQueue(message.GetType());

            var text = _messageSerializer.Serialize(message);

            await client.SendMessageAsync(text);
        }
    }
}
