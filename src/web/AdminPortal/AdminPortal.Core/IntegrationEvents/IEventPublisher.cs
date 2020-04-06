using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IIntegrationEvent;
    }
}
