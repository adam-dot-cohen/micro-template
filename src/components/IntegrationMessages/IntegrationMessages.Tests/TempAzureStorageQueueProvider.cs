﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.IntegrationMessages.AzureStorageQueue;
using Laso.IntegrationMessages.Tests.Extensions;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;

namespace Laso.IntegrationMessages.Tests
{
    public class TempAzureStorageQueueProvider : AzureStorageQueueProvider, IAsyncDisposable
    {
        private const string TestConnectionString = "DefaultEndpointsProtocol=https;AccountName=uedevstorage;AccountKey=K0eMUJoAG5MmTigJX2NTYrRw3k0M6T9qrOIDZQBKOnmt+eTzCcdWoMkd6oUeP6yYriE1M5H6yMzzHo86KXcunQ==";
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, QueueClient> _queues = new ConcurrentDictionary<string, QueueClient>();
        private readonly ISerializer _serializer;

        public TempAzureStorageQueueProvider() : base(TestConnectionString, new AzureStorageQueueConfiguration
        {
            QueueNameFormat = $"{{MessageName}}-{Guid.NewGuid().Encode(IntegerEncoding.Base36)}"
        })
        {
            _serializer = new NewtonsoftSerializer();
        }

        public async Task<TempAzureStorageQueueMessageReceiver<T>> AddReceiver<T>(Func<T, Task> onReceive = null)
        {
            var messages = new Queue<MessageProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            var listener = new TempAzureStorageQueueMessageListener<T>(messages, semaphore, this, async (x, cancellationToken) =>
            {
                if (onReceive != null)
                    await onReceive(x);
            }, _serializer);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureStorageQueueMessageReceiver<T>(this, messages, semaphore, _cancellationToken.Token, _serializer);
        }

        public IMessageSender GetSender()
        {
            return new AzureStorageQueueMessageSender(this, _serializer);
        }

        protected override Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_queues.GetOrAdd(queueName, x =>
            {
                var queueTask = base.GetQueue(queueName, cancellationToken);

                queueTask.Wait(cancellationToken);

                return queueTask.Result;
            }));
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationToken.Cancel();

            await Task.WhenAll(_queues.Values.Select(queue => queue.DeleteAsync()));
        }

        private class TempAzureStorageQueueMessageListener<T> : AzureStorageQueueMessageListener<T>
        {
            private readonly Queue<MessageProcessingResult<T>> _messages;
            private readonly SemaphoreSlim _semaphore;

            public TempAzureStorageQueueMessageListener(Queue<MessageProcessingResult<T>> messages,
                SemaphoreSlim semaphore,
                AzureStorageQueueProvider queueProvider,
                Func<T, CancellationToken, Task> messageHandler,
                ISerializer serializer) : base(queueProvider, messageHandler, serializer, serializer, pollingDelay: TimeSpan.Zero)
            {
                _messages = messages;
                _semaphore = semaphore;
            }

            protected override async Task<MessageProcessingResult<T>> ProcessMessage(QueueClient queue, QueueClient deadLetterQueue, QueueMessage message, CancellationToken stoppingToken)
            {
                var result = await base.ProcessMessage(queue, deadLetterQueue, message, stoppingToken);

                _messages.Enqueue(result);
                _semaphore.Release();

                return result;
            }
        }
    }

    public class TempAzureStorageQueueMessageReceiver<T>
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly Queue<MessageProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;
        private readonly ISerializer _serializer;

        public TempAzureStorageQueueMessageReceiver(AzureStorageQueueProvider queueProvider, Queue<MessageProcessingResult<T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken, ISerializer serializer)
        {
            _queueProvider = queueProvider;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
            _serializer = serializer;
        }

        public async Task<MessageProcessingResult<T>> WaitForMessage(TimeSpan? timeout = null)
        {
            await _semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(10), _cancellationToken);

            return _messages.Dequeue();
        }

        public async Task<(DeadLetterQueueMessage DeadLetterMessage, T Message)> WaitForDeadLetterMessage(TimeSpan? timeout = null)
        {
            var deadLetterQueue = await _queueProvider.GetDeadLetterQueue(_cancellationToken);

            var message = default(DeadLetterQueueMessage);

            var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));

            await using (_cancellationToken.Register(() => cancellationTokenSource.Cancel()))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var messages = await deadLetterQueue.ReceiveMessagesAsync(cancellationTokenSource.Token);

                    if (messages.Value.Any())
                    {
                        message = _serializer.Deserialize<DeadLetterQueueMessage>(messages.Value.First().MessageText);

                        break;
                    }
                }
            }

            return (message, message != null ? _serializer.Deserialize<T>(message.Text) : default);
        }
    }
}