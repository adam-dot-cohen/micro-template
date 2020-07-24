using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Management;
using Laso.IntegrationEvents.Tests.Extensions;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using AzureServiceBusConfiguration = Laso.IntegrationEvents.AzureServiceBus.Preview.AzureServiceBusConfiguration;
using AzureServiceBusTopicProvider = Laso.IntegrationEvents.AzureServiceBus.Preview.AzureServiceBusTopicProvider;
using RuleDescription = Azure.Messaging.ServiceBus.Management.RuleDescription;
using TopicDescription = Azure.Messaging.ServiceBus.Management.TopicDescription;

namespace Laso.IntegrationEvents.Tests.Preview
{
    public class TempAzureServiceBusTopicProvider : AzureServiceBusTopicProvider, IAsyncDisposable
    {
        private const string ConnectionString = "Endpoint=sb://uedevbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wMR2JIehLNUupAZg9F2HIr1Wz0JRi+0kh7A/n8d+oME=";

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<Type, TopicDescription> _topics = new ConcurrentDictionary<Type, TopicDescription>();

        public TempAzureServiceBusTopicProvider() : base(new AzureServiceBusConfiguration
        {
            TopicNameFormat = $"{{EventName}}_{Guid.NewGuid().Encode(IntegerEncoding.Base36)}"
        }, ConnectionString) { }

        public async Task<TempAzureServiceBusSubscription<T>> AddSubscription<T>(string subscriptionName = null, string sqlFilter = null, Func<T, Task> onReceive = null)
        {
            var messages = new Queue<AzureServiceBus.Preview.EventProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            subscriptionName ??= Guid.NewGuid().Encode(IntegerEncoding.Base36);

            async Task EventHandler(T x, CancellationToken y)
            {
                if (onReceive != null) await onReceive(x);
            }

            var serializer = new NewtonsoftSerializer();

            var listener = new TempAzureServiceBusSubscriptionEventListener<T>(messages, semaphore, this, subscriptionName, EventHandler, serializer, sqlFilter);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureServiceBusSubscription<T>(this, subscriptionName, messages, semaphore, serializer, _cancellationToken.Token);
        }

        protected override Task<TopicDescription> GetTopicDescription(ServiceBusManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            return Task.FromResult(_topics.GetOrAdd(eventType, x =>
            {
                var topicTask = base.GetTopicDescription(managementClient, eventType, cancellationToken);

                topicTask.Wait(cancellationToken);

                return topicTask.Result;
            }));
        }

        public async Task<string> GetSqlFilter(Type eventType, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ServiceBusManagementClient(ConnectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(eventType), cancellationToken);

            var rules = managementClient.GetRulesAsync(topic.Value.Name, subscriptionName, cancellationToken).ToEnumerable();

            return (rules.FirstOrDefault(x => x.Name == RuleDescription.DefaultRuleName)?.Filter as SqlRuleFilter)?.SqlExpression;
        }

        public ServiceBusReceiver GetDeadLetterClient(Type eventType, string subscriptionName)
        {
            var client = new ServiceBusClient(ConnectionString);

            return client.CreateDeadLetterReceiver(GetTopicName(eventType), subscriptionName);
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationToken.Cancel();

            var managementClient = new ServiceBusManagementClient(ConnectionString);

            await Task.WhenAll(_topics.Values
                .Select(topic => managementClient.DeleteTopicAsync(topic.Name))
                .ToArray());
        }

        private class TempAzureServiceBusSubscriptionEventListener<T> : AzureServiceBus.Preview.AzureServiceBusSubscriptionEventListener<T>
        {
            private readonly Queue<AzureServiceBus.Preview.EventProcessingResult<T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureServiceBusSubscriptionEventListener(
                Queue<AzureServiceBus.Preview.EventProcessingResult<T>> messages,
                SemaphoreSlim semaphore,
                AzureServiceBusTopicProvider topicProvider,
                string subscriptionName,
                Func<T, CancellationToken, Task> eventHandler,
                ISerializer serializer,
                string sqlFilter) : base(topicProvider, subscriptionName, eventHandler, serializer, sqlFilter)
            {
                _messages = messages;
                _semaphore = semaphore;
            }

            protected override async Task<AzureServiceBus.Preview.EventProcessingResult<T>> ProcessEvent(ProcessMessageEventArgs args, CancellationToken stoppingToken)
            {
                var result = await base.ProcessEvent(args, stoppingToken);

                _messages.Enqueue(result);
                _semaphore.Release();

                return result;
            }
        }
    }

    public class TempAzureServiceBusSubscription<T>
    {
        private readonly TempAzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Queue<AzureServiceBus.Preview.EventProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly ISerializer _serializer;
        private readonly CancellationToken _cancellationToken;

        public TempAzureServiceBusSubscription(TempAzureServiceBusTopicProvider topicProvider, string subscriptionName, Queue<AzureServiceBus.Preview.EventProcessingResult<T>> messages, SemaphoreSlim semaphore, ISerializer serializer, CancellationToken cancellationToken)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _messages = messages;
            _semaphore = semaphore;
            _serializer = serializer;
            _cancellationToken = cancellationToken;
        }

        public async Task<AzureServiceBus.Preview.EventProcessingResult<T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.TryDequeue(out var message) ? message : default;
        }

        public async Task<T> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            T @event;

            await using (var client = _topicProvider.GetDeadLetterClient(typeof(T), _subscriptionName))
            {
                var message = await client.ReceiveMessageAsync(timeout, _cancellationToken);

                @event = _serializer.DeserializeFromUtf8Bytes<T>(message.Body.AsBytes().ToArray());

                await client.CompleteMessageAsync(message, _cancellationToken);
            }

            return @event;
        }

        public async Task<string> GetFilter()
        {
            return await _topicProvider.GetSqlFilter(typeof(T), _subscriptionName, _cancellationToken);
        }
    }
}
