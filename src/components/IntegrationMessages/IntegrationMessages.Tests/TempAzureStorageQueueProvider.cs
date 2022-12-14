using System;
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
        private const string TestConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, QueueClient> _queues = new ConcurrentDictionary<string, QueueClient>();
        private readonly ISerializer _serializer;

        public TempAzureStorageQueueProvider() : base(new AzureStorageQueueConfiguration
        {
            QueueNameFormat = $"{{MessageName}}-{Guid.NewGuid().Encode(IntegerEncoding.Base36)}"
        }, TestConnectionString)
        {
            _serializer = new NewtonsoftSerializer();
        }

        public async Task<TempAzureStorageQueueMessageReceiver<T>> GetQueue<T>(Func<T, Task> onReceive = null) where T : IIntegrationMessage
        {
            var messages = new Queue<MessageProcessingResult<T>>();
            var semaphore = new SemaphoreSlim(0);

            async Task EventHandler(T x, CancellationToken y)
            {
                if (onReceive != null) await onReceive(x);
            }

            var listenerMessageHandler = new DefaultListenerMessageHandler<T>(() => new ListenerMessageHandlerContext<T>(EventHandler), _serializer);

            var listener = new TempAzureStorageQueueMessageListener<T>(messages, semaphore, this, listenerMessageHandler, _serializer);

            await listener.Open(_cancellationToken.Token);

            return new TempAzureStorageQueueMessageReceiver<T>(this, messages, semaphore, _cancellationToken.Token, _serializer);
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

        public async Task<QueueClient> GetDeadLetterQueue(CancellationToken cancellationToken = default)
        {
            return await GetQueue("DeadLetter", cancellationToken);
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

            public TempAzureStorageQueueMessageListener(
                Queue<MessageProcessingResult<T>> messages,
                SemaphoreSlim semaphore,
                AzureStorageQueueProvider queueProvider,
                IListenerMessageHandler<T> listenerMessageHandler,
                ISerializer deadLetterSerializer) : base(queueProvider, listenerMessageHandler, deadLetterSerializer, pollingDelay: TimeSpan.Zero)
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

    public class TempAzureStorageQueueMessageReceiver<T> where T : IIntegrationMessage
    {
        private readonly TempAzureStorageQueueProvider _queueProvider;
        private readonly Queue<MessageProcessingResult<T>> _messages;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;
        private readonly ISerializer _serializer;

        public TempAzureStorageQueueMessageReceiver(TempAzureStorageQueueProvider queueProvider, Queue<MessageProcessingResult<T>> messages, SemaphoreSlim semaphore, CancellationToken cancellationToken, ISerializer serializer)
        {
            _queueProvider = queueProvider;
            _messages = messages;
            _semaphore = semaphore;
            _cancellationToken = cancellationToken;
            _serializer = serializer;
        }

        public TempAzureStorageQueueMessageSender<T> GetSender()
        {
            var serializer = new DefaultMessageSerializer(_serializer);

            return new TempAzureStorageQueueMessageSender<T>(new AzureStorageQueueMessageSender(_queueProvider, serializer));
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

    public class TempAzureStorageQueueMessageSender<T> where T : IIntegrationMessage
    {
        private readonly IMessageSender _messageSender;

        public TempAzureStorageQueueMessageSender(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        public Task Send(T message)
        {
            return _messageSender.Send(message);
        }
    }
}