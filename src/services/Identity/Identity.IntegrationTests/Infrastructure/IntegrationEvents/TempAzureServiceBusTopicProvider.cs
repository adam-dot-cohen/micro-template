using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.IntegrationEvents;
using Microsoft.Azure.ServiceBus.Management;

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

        public async Task<TempAzureServiceBusSubscription<T>> AddSubscription<T>(Expression<Func<T, bool>> filter = null)
        {
            var messages = new Queue<T>();
            var semaphore = new SemaphoreSlim(0);

            var listener = new AzureServiceBusSubscriptionEventListener<T>(this, Guid.NewGuid().Encode(Encoding.Base36), (x, y) =>
            {
                messages.Enqueue(x);
                semaphore.Release();

                return Task.CompletedTask;
            }, filter);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureServiceBusSubscription<T>(messages, semaphore, _cancellationToken.Token);
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
    }

    public class TempAzureServiceBusSubscription<T>
    {
        private readonly Queue<T> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;

        public TempAzureServiceBusSubscription(Queue<T> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
        }

        public async Task<T> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.TryDequeue(out var message) ? message : default;
        }
    }
}
