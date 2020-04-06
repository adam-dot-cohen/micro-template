using System;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class EventProcessingResult<TMessage, T>
    {
        public TMessage Message { get; set; }
        public T Event { get; set; }
        public Exception Exception { get; set; }
        public Exception SecondaryException { get; set; }
        public bool WasDeadLettered { get; set; }
        public bool WasAbandoned { get; set; }
    }
}