using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Infrastructure.Extensions;
using Laso.AdminPortal.Infrastructure.IntegrationEvents;

namespace Laso.AdminPortal.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class TempAzureStorageQueueProvider : AzureStorageQueueProvider, IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, QueueClient> _queues = new ConcurrentDictionary<string, QueueClient>();

        public TempAzureStorageQueueProvider() : base(new AzureStorageQueueOptions
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==",
            QueueNameFormat = $"{{EventName}}-{Guid.NewGuid().Encode(Encoding.Base36)}"
        }) { }

        public async Task<TempAzureStorageQueueEventSubscription<T>> AddSubscription<T>()
        {
            var messages = new Queue<T>();
            var semaphore = new SemaphoreSlim(0);

            var listener = new AzureStorageQueueEventListener<T>(this, (x, cancellationToken) =>
            {
                messages.Enqueue(x);
                semaphore.Release();

                return Task.CompletedTask;
            }, null);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureStorageQueueEventSubscription<T>(messages, semaphore, _cancellationToken.Token);
        }

        protected override Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_queues.GetOrAdd(queueName, x => base.GetQueue(queueName, cancellationToken).With(y => y.Wait(cancellationToken)).Result));
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();

            Task.WaitAll(_queues.Values
                .Select(queue => queue.DeleteAsync())
                .Cast<Task>()
                .ToArray());
        }
    }

    public class TempAzureStorageQueueEventSubscription<T>
    {
        private readonly Queue<T> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;

        public TempAzureStorageQueueEventSubscription(Queue<T> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
        }

        public async Task<T> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(30), _cancellationToken);

            return _messages.Dequeue();
        }
    }
}