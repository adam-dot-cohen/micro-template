using System.Threading.Tasks;

namespace Laso.Provisioning.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish(IIntegrationEvent @event);
    }

    public interface IIntegrationEvent { }
}
