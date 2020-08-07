using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IO.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public interface IListenerMessageHandler<T>
    {
        Task Handle(Message message, EventProcessingResult<T> result, CancellationToken cancellationToken);
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

        public async Task Handle(Message message, EventProcessingResult<T> result, CancellationToken cancellationToken)
        {
            result.Event = _serializer.DeserializeFromUtf8Bytes<T>(result.Message.Body);

            using (result.Context = _createContext())
                await result.Context.EventHandler(result.Event, cancellationToken);
        }
    }
}