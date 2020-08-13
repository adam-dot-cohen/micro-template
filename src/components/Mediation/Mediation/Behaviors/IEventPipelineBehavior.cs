using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Laso.Mediation.Behaviors
{
    public interface IEventPipelineBehavior<in TEvent> where TEvent : IEvent
    {
        Task<EventResponse> Handle(TEvent notification, CancellationToken cancellationToken, RequestHandlerDelegate<EventResponse> next);
    }
}