using System;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class EventProcessingResult<TMessage, T>
    {
        public TMessage Message { get; set; }
        public T DeserializedMessage { get; set; }
        public Exception Exception { get; set; }
        public Exception SecondaryException { get; set; }
        public bool WasDeadLettered { get; set; }
        public bool WasAbandoned { get; set; }
    }
}