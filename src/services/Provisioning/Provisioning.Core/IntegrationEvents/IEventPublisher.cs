using System.Threading.Tasks;

namespace Laso.Provisioning.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IIntegrationEvent;
    }

    public interface IIntegrationEvent { }
}
