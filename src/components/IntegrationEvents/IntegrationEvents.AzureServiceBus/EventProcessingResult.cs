using System;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class EventProcessingResult<T>
    {
        public Message Message { get; set; }
        public T Event { get; set; }
        public Exception Exception { get; set; }
        public Exception SecondaryException { get; set; }
        public bool WasDeadLettered { get; set; }
        public bool WasAbandoned { get; set; }
    }
}