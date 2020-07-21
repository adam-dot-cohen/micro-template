using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.Tests.Extensions;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.IntegrationEvents.Tests
{
    public class TempAzureServiceBusTopicProvider : AzureServiceBusTopicProvider, IAsyncDisposable
    {
        private const string ConnectionString = "Endpoint=sb://uedevbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wMR2JIehLNUupAZg9F2HIr1Wz0JRi+0kh7A/n8d+oME=";

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<Type, TopicDescription> _topics = new ConcurrentDictionary<Type, TopicDescription>();

        public TempAzureServiceBusTopicProvider() : base(ConnectionString, new AzureServiceBusConfiguration
        {
            TopicNameFormat = $"{{EventName}}_{Guid.NewGuid().Encode(IntegerEncoding.Base36)}"
        }) { }

        public async Task<TempAzureServiceBusSubscription<T>> AddSubscription<T>(string subscriptionName = null, string sqlFilter = null, Func<T, Task> onReceive = null)
        {
            var messages = new Queue<EventProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            subscriptionName ??= Guid.NewGuid().Encode(IntegerEncoding.Base36);

            async Task EventHandler(T x, CancellationToken y)
            {
                if (onReceive != null) await onReceive(x);
            }

            var listener = new TempAzureServiceBusSubscriptionEventListener<T>(messages, semaphore, this, subscriptionName, EventHandler, sqlFilter);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureServiceBusSubscription<T>(this, subscriptionName, messages, semaphore, _cancellationToken.Token);
        }

        protected override Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_topics.GetOrAdd(eventType, x =>
            {
                var topicTask = base.GetTopicDescription(managementClient, eventType, cancellationToken);

                topicTask.Wait(cancellationToken);

                return topicTask.Result;
            }));
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationToken.Cancel();

            var managementClient = new ManagementClient(ConnectionString);

            await Task.WhenAll(_topics.Values
                .Select(topic => managementClient.DeleteTopicAsync(topic.Path))
                .ToArray());
        }

        private class TempAzureServiceBusSubscriptionEventListener<T> : AzureServiceBusSubscriptionEventListener<T>
        {
            private readonly Queue<EventProcessingResult<T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureServiceBusSubscriptionEventListener(
                Queue<EventProcessingResult<T>> messages,
                SemaphoreSlim semaphore,
                AzureServiceBusTopicProvider topicProvider,
                string subscriptionName,
                Func<T, CancellationToken, Task> eventHandler,
                string sqlFilter) : base(topicProvider, subscriptionName, eventHandler, new NewtonsoftSerializer(), sqlFilter)
            {
                _messages = messages;
                _semaphore = semaphore;
            }

            protected override async Task<EventProcessingResult<T>> ProcessEvent(IReceiverClient client, Message message, CancellationToken stoppingToken)
            {
                var result = await base.ProcessEvent(client, message, stoppingToken);

                _messages.Enqueue(result);
                _semaphore.Release();

                return result;
            }
        }
    }

    public class TempAzureServiceBusSubscription<T>
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Queue<EventProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;

        public TempAzureServiceBusSubscription(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Queue<EventProcessingResult<T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
        }

        public async Task<EventProcessingResult<T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.TryDequeue(out var message) ? message : default;
        }

        public async Task<T> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            var client = await _topicProvider.GetDeadLetterClient(typeof(T), _subscriptionName, _cancellationToken);

            var options = new MessageHandlerOptions(x => Task.CompletedTask) { AutoComplete = false };

            var @event = default(T);
            var semaphore = new SemaphoreSlim(0, 1);

            client.RegisterMessageHandler(async (x, y) =>
            {
                @event = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(x.Body));

                await client.CompleteAsync(x.SystemProperties.LockToken);

                semaphore.Release();
            }, options);

            await semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            await client.CloseAsync();

            return @event;
        }

        public async Task<string> GetFilter()
        {
            return await _topicProvider.GetSqlFilter(typeof(T), _subscriptionName, _cancellationToken);
        }
    }
}
