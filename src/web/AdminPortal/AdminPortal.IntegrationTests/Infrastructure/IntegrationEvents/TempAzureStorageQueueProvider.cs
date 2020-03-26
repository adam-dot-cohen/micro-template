using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Infrastructure.Extensions;
using Laso.AdminPortal.Infrastructure.IntegrationEvents;

namespace Laso.AdminPortal.IntegrationTests.Infrastructure.IntegrationEvents
{
    public class TempAzureStorageQueueProvider : AzureStorageQueueProvider, IAsyncDisposable
    {
        private const string TestConnectionString = "DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==";
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, QueueClient> _queues = new ConcurrentDictionary<string, QueueClient>();

        public TempAzureStorageQueueProvider() : base(TestConnectionString, new AzureStorageQueueOptions
        {
            QueueNameFormat = $"{{EventName}}-{Guid.NewGuid().Encode(Encoding.Base36)}"
        }) { }

        public async Task<TempAzureStorageQueueEventReceiver<T>> AddReceiver<T>(Func<T, Task> onReceive = null)
        {
            var messages = new Queue<EventProcessingResult<QueueMessage, T>>();
            var semaphore = new SemaphoreSlim(0);

            var listener = new TempAzureStorageQueueEventListener<T>(messages, semaphore, this, async (x, cancellationToken) =>
            {
                if (onReceive != null)
                    await onReceive(x);
            });

            await listener.Open(_cancellationToken.Token);

            return new TempAzureStorageQueueEventReceiver<T>(this, messages, semaphore, _cancellationToken.Token);
        }

        protected override Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_queues.GetOrAdd(queueName, x => base.GetQueue(queueName, cancellationToken).With(y => y.Wait(cancellationToken)).Result));
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationToken.Cancel();

            await Task.WhenAll(_queues.Values.Select(queue => queue.DeleteAsync()));
        }

        private class TempAzureStorageQueueEventListener<T> : AzureStorageQueueEventListener<T>
        {
            private readonly Queue<EventProcessingResult<QueueMessage, T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureStorageQueueEventListener(
                Queue<EventProcessingResult<QueueMessage, T>> messages,
                SemaphoreSlim semaphore,
                AzureStorageQueueProvider queueProvider,
                Func<T, CancellationToken, Task> eventHandler) : base(queueProvider, eventHandler, pollingDelay: TimeSpan.Zero)
            {
                _messages = messages;
                _semaphore = semaphore;
            }

            protected override async Task<EventProcessingResult<QueueMessage, T>> ProcessEvent(QueueClient queue, QueueClient deadLetterQueue, QueueMessage message, CancellationToken stoppingToken)
            {
                var result = await base.ProcessEvent(queue, deadLetterQueue, message, stoppingToken);

                _messages.Enqueue(result);
                _semaphore.Release();

                return result;
            }
        }
    }

    public class TempAzureStorageQueueEventReceiver<T>
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly Queue<EventProcessingResult<QueueMessage, T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;

        public TempAzureStorageQueueEventReceiver(AzureStorageQueueProvider queueProvider, Queue<EventProcessingResult<QueueMessage, T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            _queueProvider = queueProvider;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
        }

        public async Task<EventProcessingResult<QueueMessage, T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.Dequeue();
        }

        public async Task<T> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            var deadLetterQueue = await _queueProvider.GetDeadLetterQueue(_cancellationToken);

            var message = default(T);

            var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));

            await using (_cancellationToken.Register(() => cancellationTokenSource.Cancel()))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var messages = await deadLetterQueue.ReceiveMessagesAsync(cancellationTokenSource.Token);

                    if (messages.Value.Any())
                    {
                        var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};

                        message = JsonSerializer.Deserialize<T>(messages.Value.First().MessageText, options);

                        break;
                    }
                }
            }

            return message;
        }

        public ICollection<EventProcessingResult<QueueMessage, T>> GetAllMessageResults()
        {
            return _messages.ToList();
        }
    }
}