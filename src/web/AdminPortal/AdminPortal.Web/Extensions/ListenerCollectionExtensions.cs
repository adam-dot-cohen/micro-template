using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Hosting;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IntegrationMessages.AzureStorageQueue;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Web.Extensions
{
    internal static class ListenerCollectionExtensions
    {
        private const string SubscriptionPrefix = "AdminPortal.Web";

        public static void AddCloudSubscription<T>(
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

        public static void AddSubscription<T>(
            this ListenerCollection listenerCollection,
            string subscriptionName = null,
            Func<IServiceProvider, ISerializer> getSerializer = null)
            where T : IEvent
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
                            async (@event, cancellationToken) => await scope.ServiceProvider
                                .GetRequiredService<IMediator>()
                                .Publish(@event ?? throw new ArgumentNullException(nameof(@event)), cancellationToken),
                            scope);
                    }, getSerializer != null ? getSerializer(sp) : sp.GetRequiredService<IJsonSerializer>()),
                    logger: sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<T>>>());

                return listener.Open;
            });
        }

        public static void AddReceiver<T>(
            this ListenerCollection listenerCollection,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler,
            Func<IServiceProvider, ISerializer> getSerializer = null)
        {
            listenerCollection.Add(sp =>
            {
                var defaultSerializer = sp.GetRequiredService<IJsonSerializer>();

                var listener = new AzureStorageQueueMessageListener<T>(
                    sp.GetRequiredService<AzureStorageQueueProvider>(),
                    getEventHandler(sp),
                    getSerializer != null ? getSerializer(sp) : defaultSerializer,
                    defaultSerializer,
                    logger: sp.GetRequiredService<ILogger<AzureStorageQueueMessageListener<T>>>());

                return listener.Open;
            });
        }
    }
}