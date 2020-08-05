using System.Threading.Tasks;

namespace Laso.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event, string topicName = null) where T : IIntegrationEvent;
    }
}
