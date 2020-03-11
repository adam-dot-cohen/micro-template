using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class NopServiceBusEventPublisher : IEventPublisher
    {
        public Task Publish(object @event)
        {
            return Task.CompletedTask;
        }
    }
}