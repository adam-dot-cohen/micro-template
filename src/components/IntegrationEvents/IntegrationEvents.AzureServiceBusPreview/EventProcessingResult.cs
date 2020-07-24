using System;
using Azure.Messaging.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public class EventProcessingResult<T>
    {
        public ServiceBusReceivedMessage Message { get; set; }
        public T Event { get; set; }
        public Exception Exception { get; set; }
        public Exception SecondaryException { get; set; }
        public bool WasDeadLettered { get; set; }
        public bool WasAbandoned { get; set; }
    }
}