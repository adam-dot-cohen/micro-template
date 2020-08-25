using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Laso.IO.Serialization;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public interface IListenerMessageHandler<T>
    {
        Task Handle(QueueMessage message, MessageProcessingResult<T> result, CancellationToken cancellationToken);
    }

    public class DefaultListenerMessageHandler<T> : IListenerMessageHandler<T>
    {
        private readonly Func<ListenerMessageHandlerContext<T>> _createContext;
        private readonly ISerializer _serializer;

        public DefaultListenerMessageHandler(Func<ListenerMessageHandlerContext<T>> createContext, ISerializer serializer)
        {
            _createContext = createContext;
            _serializer = serializer;
        }

        public async Task Handle(QueueMessage message, MessageProcessingResult<T> result, CancellationToken cancellationToken)
        {
            result.Message = _serializer.Deserialize<T>(message.MessageText);

            using (result.Context = _createContext())
                await result.Context.EventHandler(result.Message, cancellationToken);
        }
    }
}