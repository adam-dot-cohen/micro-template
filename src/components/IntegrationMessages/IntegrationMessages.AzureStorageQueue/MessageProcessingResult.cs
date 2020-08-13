using System;
using Azure.Storage.Queues.Models;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class MessageProcessingResult<T>
    {
        public QueueMessage QueueMessage { get; set; }
        public T Message { get; set; }
        public Exception Exception { get; set; }
        public Exception SecondaryException { get; set; }
        public bool WasDeadLettered { get; set; }
        public bool WasAbandoned { get; set; }
    }
}