using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents.AzureServiceBus;
using CloudNative.CloudEvents.Json;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus.CloudEvents
{
    public class CloudEventListenerMessageHandler<T> : IListenerMessageHandler<T>
    {
        private readonly Func<string, string, ListenerMessageHandlerContext<T>> _createContext;
        private readonly NewtonsoftSerializer _serializer;

        public CloudEventListenerMessageHandler(Func<string, string, ListenerMessageHandlerContext<T>> createContext, NewtonsoftSerializer serializer)
        {
            _createContext = createContext;
            _serializer = serializer;
        }

        public async Task Handle(Message message, EventProcessingResult<T> result, CancellationToken cancellationToken)
        {
            //todo
            //var cloudEvent = message.ToCloudEvent(new JsonCloudEventFormatter<T>(_serializer.GetSettings()), new DistributedTracingExtension());

            //result.Event = (T) cloudEvent.Data;

            //var tracing = cloudEvent.Extension<DistributedTracingExtension>();

            //using (result.Context = _createContext(tracing?.TraceParent, tracing?.TraceState))
                await result.Context.EventHandler(result.Event, cancellationToken);
        }
    }
}