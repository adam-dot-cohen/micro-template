using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace Laso.Identity.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class TempAzureServiceBusTopicProvider : AzureServiceBusTopicProvider, IDisposable
    {
        private const string ConnectionString = "Endpoint=sb://uedevbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wMR2JIehLNUupAZg9F2HIr1Wz0JRi+0kh7A/n8d+oME=";

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<Type, TopicDescription> _topics = new ConcurrentDictionary<Type, TopicDescription>();

        public TempAzureServiceBusTopicProvider() : base(ConnectionString, new AzureServiceBusConfiguration
        {
            TopicNameFormat = $"{{EventName}}_{Guid.NewGuid().Encode(Encoding.Base36)}"
        }) { }

        public async Task<TempAzureServiceBusSubscription<T>> AddSubscription<T>(string subscriptionName = null, Expression<Func<T, bool>> filter = null, Func<T, Task> onReceive = null)
        {
            var messages = new Queue<EventProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            subscriptionName ??= Guid.NewGuid().Encode(Encoding.Base36);

            var listener = new TempAzureServiceBusSubscriptionEventListener<T>(messages, semaphore, this, subscriptionName, async (x, y) =>
            {
                if (onReceive != null)
                    await onReceive(x);
            }, filter);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureServiceBusSubscription<T>(
                () => GetDeadLetterClient(typeof(T), subscriptionName, _cancellationToken.Token),
                () => GetSqlFilter(typeof(T), subscriptionName, _cancellationToken.Token),
                messages,
                semaphore,
                _cancellationToken.Token);
        }

        protected override Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_topics.GetOrAdd(eventType, x => base.GetTopicDescription(managementClient, eventType, cancellationToken).With(y => y.Wait(cancellationToken)).Result));
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();

            var managementClient = new ManagementClient(ConnectionString);

            Task.WaitAll(_topics.Values
                .Select(topic => managementClient.DeleteTopicAsync(topic.Path))
                .ToArray());
        }

        private class TempAzureServiceBusSubscriptionEventListener<T> : AzureServiceBusSubscriptionEventListener<T>
        {
            private readonly Queue<EventProcessingResult<T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureServiceBusSubscriptionEventListener(Queue<EventProcessingResult<T>> messages, SemaphoreSlim semaphore, AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, CancellationToken, Task> eventHandler, Expression<Func<T, bool>> filter, ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null) : base(topicProvider, subscriptionName, eventHandler, filter, logger)
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
        private readonly Func<Task<MessageReceiver>> _getDeadLetterReceiver;
        private readonly Func<Task<string>> _getFilter;
        private readonly Queue<EventProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;

        public TempAzureServiceBusSubscription(Func<Task<MessageReceiver>> getDeadLetterReceiver, Func<Task<string>> getFilter, Queue<EventProcessingResult<T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            _getDeadLetterReceiver = getDeadLetterReceiver;
            _getFilter = getFilter;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
        }

        public ICollection<EventProcessingResult<T>> GetAllMessageResults()
        {
            return _messages.ToList();
        }

        public async Task<EventProcessingResult<T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.TryDequeue(out var message) ? message : default;
        }

        public async Task<T> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            var client = await _getDeadLetterReceiver();

            var options = new MessageHandlerOptions(x => Task.CompletedTask)
            {
                AutoComplete = false,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(1),
                MaxConcurrentCalls = 1
            };

            var messages = new Queue<T>();
            var semaphore = new SemaphoreSlim(0);

            client.RegisterMessageHandler(async (x, y) =>
            {
                var @event = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(x.Body));

                await client.CompleteAsync(x.SystemProperties.LockToken);

                messages.Enqueue(@event);
                semaphore.Release();
            }, options);

            await semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            await client.CloseAsync();

            return messages.TryDequeue(out var message) ? message : default;
        }

        public async Task<string> GetFilter()
        {
            return await _getFilter();
        }
    }
}
