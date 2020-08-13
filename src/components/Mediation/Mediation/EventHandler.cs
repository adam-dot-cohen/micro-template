using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.Mediation
{
    public interface IEventHandler<in TEvent>
        where TEvent : IEvent
    {
        Task<EventResponse> Handle(TEvent notification, CancellationToken cancellationToken);
    }

    public abstract class EventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        public abstract Task<EventResponse> Handle(TEvent request, CancellationToken cancellationToken);

        protected static EventResponse Succeeded() => EventResponse.Succeeded();
        protected static EventResponse Failed(string message) => EventResponse.Failed(message);
        protected static EventResponse Failed(string key, string message) => EventResponse.Failed(key, message);
        protected static EventResponse Failed(ValidationMessage message) => EventResponse.Failed(message);
        protected static EventResponse Failed(IEnumerable<ValidationMessage> messages) => EventResponse.Failed(messages);
        protected static EventResponse Failed(Exception exception) => EventResponse.Failed(exception);
        protected static EventResponse Failed(Response response) => EventResponse.Failed(response);
    }
}
