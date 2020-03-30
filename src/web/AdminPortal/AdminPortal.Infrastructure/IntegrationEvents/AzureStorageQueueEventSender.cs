using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.IO.Serialization;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueEventSender : IEventSender
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly ISerializer _serializer;

        public AzureStorageQueueEventSender(AzureStorageQueueProvider queueProvider, ISerializer serializer)
        {
            _queueProvider = queueProvider;
            _serializer = serializer;
        }

        public async Task Send<T>(T @event) where T : IIntegrationEvent
        {
            var client = await _queueProvider.GetQueue(@event.GetType());

            var text = _serializer.Serialize(@event);

            await client.SendMessageAsync(text);
        }
    }
}
