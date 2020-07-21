using System.Threading.Tasks;

namespace Laso.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IIntegrationEvent;
    }
}
