using System.Threading.Tasks;
using Laso.Identity.Core.Messaging;

namespace Laso.Identity.Infrastructure.Eventing
{
    public class NopServiceBusEventPublisher : IEventPublisher
    {
        public Task Publish(object @event)
        {
            return Task.CompletedTask;
        }
    }
}