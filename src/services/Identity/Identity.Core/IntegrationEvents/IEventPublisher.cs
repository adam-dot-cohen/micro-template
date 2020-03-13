using System.Threading.Tasks;

namespace Laso.Identity.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish(IIntegrationEvent @event);
    }
}
