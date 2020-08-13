using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Hosting;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laso.Scheduling.Api.Extensions
{
    internal static class ListenerCollectionExtensions
    {
        private const string SubscriptionPrefix = "Scheduling.Api";

        public static void AddSubscription<T>(
            this ListenerCollection listenerCollection,
            Func<IServiceProvider, AzureServiceBusTopicProvider> getTopicProvider,
            string topicName,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler)
        {
            listenerCollection.Add(sp =>
            {
                var listener = new AzureServiceBusSubscriptionEventListener<T>(
                    getTopicProvider(sp),
                    $"{SubscriptionPrefix}-{typeof(T).Name}",
                    new CloudEventListenerMessageHandler<T>((traceParent, traceState) =>
                    {
                        var scope = sp.CreateScope();

                        return new ListenerMessageHandlerContext<T>(
                            getEventHandler(scope.ServiceProvider),
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