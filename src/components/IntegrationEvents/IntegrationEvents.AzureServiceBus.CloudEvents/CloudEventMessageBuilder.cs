using System;
using System.Diagnostics;
using System.Globalization;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using CloudNative.CloudEvents.Json;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents.Extensions;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Azure.ServiceBus;

namespace Laso.IntegrationEvents.AzureServiceBus.CloudEvents
{
    public class CloudEventMessageBuilder : IMessageBuilder
    {
        internal const string LasoVendorName = "laso";

        private readonly NewtonsoftSerializer _serializer;
        private readonly Uri _source;

        public CloudEventMessageBuilder(NewtonsoftSerializer serializer, Uri source)
        {
            _serializer = serializer;
            _source = source;
        }

        public Message Build<T>(T @event, string topicName) where T : IIntegrationEvent
        {
            var eventType = @event.GetType();

            var eventName = eventType.Name;

            if (eventName.EndsWith("Event", true, CultureInfo.InvariantCulture))
                eventName = eventName.Substring(0, eventName.Length - "Event".Length);

            var cloudEvent = new CloudEvent(
                $"com.{LasoVendorName}.{CamelCase(topicName)}.{CamelCase(eventName)}",
                _source,
                time: DateTime.UtcNow,
                extensions: new DistributedTracingExtension(Activity.Current.GetTraceParent()) { TraceState = Activity.Current?.TraceStateString })
            {
                Data = @event
            };

            var body = new JsonCloudEventFormatter<T>(_serializer.GetSettings()).EncodeStructuredEvent(cloudEvent, out var contentType);

            var message = new Message
            {
                Body = body,
                ContentType = contentType.MediaType,
                MessageId = cloudEvent.Id
            };

            message.UserProperties.Add("EventType", eventType.Name);

            return message;
        }

        private static string CamelCase(string topicName)
        {
            return topicName.Substring(0, 1).ToLowerInvariant() + topicName.Substring(1);
        }
    }
}
