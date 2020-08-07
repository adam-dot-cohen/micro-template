using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Laso.IO.Serialization;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public interface IListenerMessageHandler<T>
    {
        Task Handle(ServiceBusReceivedMessage message, EventProcessingResult<T> result, CancellationToken cancellationToken);
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

        public async Task Handle(ServiceBusReceivedMessage message, EventProcessingResult<T> result, CancellationToken cancellationToken)
        {
            result.Event = _serializer.DeserializeFromUtf8Bytes<T>(message.Body.AsBytes().ToArray());

            using (result.Context = _createContext())
                await result.Context.EventHandler(result.Event, cancellationToken);
        }
    }
}