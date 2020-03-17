using System.Text.Json;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueEventPublisher : IEventPublisher
    {
        private readonly AzureStorageQueueProvider _queueProvider;

        public AzureStorageQueueEventPublisher(AzureStorageQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish<T>(T @event) where T : IIntegrationEvent
        {
            var client = await _queueProvider.GetQueue(@event.GetType());

            var text = JsonSerializer.Serialize(@event);

            await client.SendMessageAsync(text);
        }
    }
}
