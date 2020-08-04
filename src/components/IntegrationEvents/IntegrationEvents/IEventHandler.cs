using System.Threading.Tasks;

namespace Laso.IntegrationEvents
{
    public interface IEventHandler<T> where T : IIntegrationEvent
    {
        Task Handle(T @event) ;
    }
}