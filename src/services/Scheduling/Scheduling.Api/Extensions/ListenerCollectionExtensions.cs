using System;
using Laso.Hosting;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laso.Scheduling.Api.Extensions
{
    internal static class ListenerCollectionExtensions
    {
        private const string SubscriptionPrefix = "Scheduling.Api";

        public static void AddSubscription<T>(
            this ListenerCollection listenerCollection,
            string topicName)
            where T : IEvent
        {
            listenerCollection.Add(sp =>
            {
                var listener = new AzureServiceBusSubscriptionEventListener<T>(
                    sp.GetRequiredService<AzureServiceBusTopicProvider>(),
                    $"{SubscriptionPrefix}-{typeof(T).Name}",
                    new CloudEventListenerMessageHandler<T>((traceParent, traceState) =>
                    {
                        var scope = sp.CreateScope();

                        return new ListenerMessageHandlerContext<T>(
                            async (@event, cancellationToken) => await scope.ServiceProvider
                                .GetRequiredService<IMediator>()
                                .Publish(@event ?? throw new ArgumentNullException(nameof(@event)), cancellationToken),
                            scope,
                            traceParent,
                            traceState);
                    }, sp.GetRequiredService<NewtonsoftSerializer>()),
                    topicName: topicName,
                    sqlFilter: $"EventType = '{typeof(T).Name}'",
                    logger: sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<T>>>());

                return listener.Open;
            });
        }
    }
}