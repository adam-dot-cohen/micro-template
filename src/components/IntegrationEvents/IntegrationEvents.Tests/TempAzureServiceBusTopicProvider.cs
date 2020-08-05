using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IntegrationEvents.Tests.Extensions;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using AzureServiceBusConfiguration = Laso.IntegrationEvents.AzureServiceBus.AzureServiceBusConfiguration;
using AzureServiceBusTopicProvider = Laso.IntegrationEvents.AzureServiceBus.AzureServiceBusTopicProvider;

namespace Laso.IntegrationEvents.Tests
{
    public class TempAzureServiceBusTopicProvider : AzureServiceBusTopicProvider, IAsyncDisposable
    {
        private const string ConnectionString = "Endpoint=sb://uedevbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wMR2JIehLNUupAZg9F2HIr1Wz0JRi+0kh7A/n8d+oME=";

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, TopicDescription> _topics = new ConcurrentDictionary<string, TopicDescription>();

        public TempAzureServiceBusTopicProvider() : base(new AzureServiceBusConfiguration
        {
            TopicNameFormat = $"{{TopicName}}_{Guid.NewGuid().Encode(IntegerEncoding.Base36)}"
        }, ConnectionString) { }

        public TempAzureServiceBusTopic<T> GetTopic<T>(string name = null, bool isCloudEvent = false) where T : IIntegrationEvent
        {
            return new TempAzureServiceBusTopic<T>(name, isCloudEvent, this, _cancellationToken.Token);
        }

        protected override Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, string topicName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_topics.GetOrAdd(topicName, x =>
            {
                var topicTask = base.GetTopicDescription(managementClient, topicName, cancellationToken);

                topicTask.Wait(cancellationToken);

                return topicTask.Result;
            }));
        }

        public async Task<string> GetSqlFilter(string topicName, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(ConnectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(topicName), cancellationToken);

            var subscription = await managementClient.GetSubscriptionAsync(topic.Path, subscriptionName, cancellationToken);

            var subscriptionClient = new SubscriptionClient(ConnectionString, subscription.TopicPath, subscription.SubscriptionName);

            var rules = await subscriptionClient.GetRulesAsync();

            return (rules.FirstOrDefault(x => x.Name == RuleDescription.DefaultRuleName)?.Filter as SqlFilter)?.SqlExpression;
        }

        public async Task<MessageReceiver> GetDeadLetterClient(string topicName, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(ConnectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(topicName), cancellationToken);

            var path = EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topic.Path, subscriptionName));

            return new MessageReceiver(ConnectionString, path);
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationToken.Cancel();

            var managementClient = new ManagementClient(ConnectionString);

            await Task.WhenAll(_topics.Values
                .Select(topic => managementClient.DeleteTopicAsync(topic.Path))
                .ToArray());
        }
    }

    public class TempAzureServiceBusTopic<T> where T : IIntegrationEvent
    {
        private readonly string _topicName;
        private readonly bool _isCloudEvent;
        private readonly TempAzureServiceBusTopicProvider _topicProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly NewtonsoftSerializer _serializer;

        public TempAzureServiceBusTopic(string topicName, bool isCloudEvent, TempAzureServiceBusTopicProvider topicProvider, CancellationToken cancellationToken)
        {
            _topicName = topicName;
            _isCloudEvent = isCloudEvent;
            _topicProvider = topicProvider;
            _cancellationToken = cancellationToken;
            _serializer = new NewtonsoftSerializer();
        }

        public async Task<TempAzureServiceBusSubscription<T>> AddSubscription(string subscriptionName = null, string sqlFilter = null, Func<T, Task> onReceive = null)
        {
            var messages = new Queue<EventProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            subscriptionName ??= Guid.NewGuid().Encode(IntegerEncoding.Base36);

            async Task EventHandler(T x, CancellationToken y)
            {
                if (onReceive != null) await onReceive(x);
            }

            var listenerMessageHandler = _isCloudEvent
                ? new CloudEventListenerMessageHandler<T>((traceParent, traceState) => new ListenerMessageHandlerContext<T>(EventHandler, traceParent: traceParent, traceState: traceState), _serializer) as IListenerMessageHandler<T>
                : new DefaultListenerMessageHandler<T>(() => new ListenerMessageHandlerContext<T>(EventHandler), _serializer);

            var listener = new TempAzureServiceBusSubscriptionEventListener(messages, semaphore, _topicProvider, _topicName, subscriptionName, listenerMessageHandler, sqlFilter);

            await listener.Open(_cancellationToken);

            return new TempAzureServiceBusSubscription<T>(_topicProvider, _topicName, subscriptionName, messages, semaphore, _cancellationToken, _serializer);
        }

        public TempAzureServiceBusEventPublisher<T> GetPublisher()
        {
            var messageBuilder = _isCloudEvent
                ? new CloudEventMessageBuilder(_serializer, new Uri("service://test")) as IMessageBuilder
                : new DefaultMessageBuilder(_serializer);

            return new TempAzureServiceBusEventPublisher<T>(new AzureServiceBusEventPublisher(_topicProvider, messageBuilder), _topicName);
        }

        private class TempAzureServiceBusSubscriptionEventListener : AzureServiceBusSubscriptionEventListener<T>
        {
            private readonly Queue<EventProcessingResult<T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureServiceBusSubscriptionEventListener(
                Queue<EventProcessingResult<T>> messages,
                SemaphoreSlim semaphore,
                AzureServiceBusTopicProvider topicProvider,
                string topicName,
                string subscriptionName,
                IListenerMessageHandler<T> listenerMessageHandler,
                string sqlFilter) : base(topicProvider, subscriptionName, listenerMessageHandler, topicName: topicName, sqlFilter: sqlFilter)
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

    public class TempAzureServiceBusEventPublisher<T> where T : IIntegrationEvent
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly string _topicName;

        public TempAzureServiceBusEventPublisher(IEventPublisher eventPublisher, string topicName)
        {
            _eventPublisher = eventPublisher;
            _topicName = topicName;
        }

        public Task Publish(T @event)
        {
            return _eventPublisher.Publish(@event, _topicName);
        }
    }

    public class TempAzureServiceBusSubscription<T> where T : IIntegrationEvent
    {
        private readonly TempAzureServiceBusTopicProvider _topicProvider;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly Queue<EventProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;
        private readonly ISerializer _serializer;

        public TempAzureServiceBusSubscription(TempAzureServiceBusTopicProvider topicProvider, string topicName, string subscriptionName, Queue<EventProcessingResult<T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken, ISerializer serializer)
        {
            _topicProvider = topicProvider;
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
            _serializer = serializer;
        }

        public async Task<EventProcessingResult<T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.TryDequeue(out var message) ? message : default;
        }

        public async Task<T> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            var client = await _topicProvider.GetDeadLetterClient(_topicName ?? typeof(T).Name, _subscriptionName, _cancellationToken);

            var options = new MessageHandlerOptions(x => Task.CompletedTask) { AutoComplete = false };

            var @event = default(T);
            var semaphore = new SemaphoreSlim(0, 1);

            client.RegisterMessageHandler(async (x, y) =>
            {
                @event = _serializer.DeserializeFromUtf8Bytes<T>(x.Body);

                await client.CompleteAsync(x.SystemProperties.LockToken);

                semaphore.Release();
            }, options);

            await semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            await client.CloseAsync();

            return @event;
        }

        public async Task<string> GetFilter()
        {
            return await _topicProvider.GetSqlFilter(_topicName ?? typeof(T).Name, _subscriptionName, _cancellationToken);
        }
    }
}
