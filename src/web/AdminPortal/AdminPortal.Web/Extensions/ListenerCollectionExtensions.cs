using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Hosting;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IntegrationMessages.AzureStorageQueue;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Web.Extensions
{
    internal static class ListenerCollectionExtensions
    {
        private const string SubscriptionPrefix = "AdminPortal.Web";

        public static void AddSubscription<T>(
            this ListenerCollection listenerCollection,
            string topicName,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler)
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

        public static void AddSubscription<T>(
            this ListenerCollection listenerCollection,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler,
            string subscriptionName = null,
            string sqlFilter = null,
            Func<IServiceProvider, ISerializer> getSerializer = null)
        {
            listenerCollection.Add(sp =>
            {
                var listener = new AzureServiceBusSubscriptionEventListener<T>(
                    sp.GetRequiredService<AzureServiceBusTopicProvider>(),
                    SubscriptionPrefix + (subscriptionName != null ? "-" + subscriptionName : ""),
                    new DefaultListenerMessageHandler<T>(() =>
                    {
                        var scope = sp.CreateScope();

                        return new ListenerMessageHandlerContext<T>(
                            getEventHandler(scope.ServiceProvider),
                            scope);
                    }, getSerializer != null ? getSerializer(sp) : sp.GetRequiredService<IJsonSerializer>()),
                    sqlFilter: sqlFilter,
                    logger: sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<T>>>());

                return listener.Open;
            });
        }

        public static void AddReceiver<T>(
            this ListenerCollection listenerCollection,
            Func<IServiceProvider, AzureStorageQueueProvider> getQueueProvider,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler,
            Func<IServiceProvider, ISerializer> getSerializer = null)
        {
            listenerCollection.Add(sp =>
            {
                var defaultSerializer = sp.GetRequiredService<IJsonSerializer>();

                var listener = new AzureStorageQueueMessageListener<T>(
                    getQueueProvider(sp),
                    getEventHandler(sp),
                    getSerializer != null ? getSerializer(sp) : defaultSerializer,
                    defaultSerializer,
                    logger: sp.GetRequiredService<ILogger<AzureStorageQueueMessageListener<T>>>());

                return listener.Open;
            });
        }
    }
}