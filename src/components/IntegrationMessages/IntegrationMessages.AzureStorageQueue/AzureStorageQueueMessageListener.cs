using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.IntegrationMessages.AzureStorageQueue.Extensions;
using Laso.IO.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class AzureStorageQueueMessageListener<T>
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly Func<T, CancellationToken, Task> _messageHandler;
        private readonly ISerializer _serializer;
        private readonly ISerializer _deadLetterSerializer;
        private readonly TimeSpan _pollingDelay;
        private readonly TimeSpan? _visibilityTimeout;
        private readonly ILogger<AzureStorageQueueMessageListener<T>> _logger;

        private Task _pollingTask;

        public AzureStorageQueueMessageListener(AzureStorageQueueProvider queueProvider,
            Func<T, CancellationToken, Task> messageHandler,
            ISerializer serializer,
            ISerializer deadLetterSerializer,
            TimeSpan? pollingDelay = null,
            TimeSpan? visibilityTimeout = null,
            ILogger<AzureStorageQueueMessageListener<T>> logger = null)
        {
            _queueProvider = queueProvider;
            _messageHandler = messageHandler;
            _serializer = serializer;
            _deadLetterSerializer = deadLetterSerializer;
            _pollingDelay = pollingDelay ?? TimeSpan.FromSeconds(5);
            _visibilityTimeout = visibilityTimeout;
            _logger = logger ?? new NullLogger<AzureStorageQueueMessageListener<T>>();
        }

        public async Task Open(CancellationToken stoppingToken)
        {
            Task<QueueClient> queue = null;
            Task<QueueClient> deadLetterQueue = null;
            var success = false;

            while (!stoppingToken.IsCancellationRequested && !success)
            {
                try
                {
                    queue = _queueProvider.GetQueue(typeof(T), stoppingToken);
                    deadLetterQueue = _queueProvider.GetQueue("DeadLetter", stoppingToken);

                    await Task.WhenAll(queue, deadLetterQueue);

                    success = true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error connecting to queue of type {typeof(T).Name}");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            // ReSharper disable twice PossibleNullReferenceException
            _pollingTask = PollQueue(queue.Result, deadLetterQueue.Result, stoppingToken);
        }

        private async Task PollQueue(QueueClient queue, QueueClient deadLetterQueue, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await queue.ReceiveMessagesAsync(visibilityTimeout: _visibilityTimeout, cancellationToken: stoppingToken);

                    await Task.WhenAll(messages.Value.Select(x => ProcessMessage(queue, deadLetterQueue, x, stoppingToken)));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error receiving queued message of type {typeof(T).Name}");
                }
                finally
                {
                    await Task.Delay(_pollingDelay, stoppingToken);
                }
            }
        }

        protected virtual async Task<MessageProcessingResult<T>> ProcessMessage(QueueClient queue, QueueClient deadLetterQueue, QueueMessage message, CancellationToken stoppingToken)
        {
            var result = new MessageProcessingResult<T> { QueueMessage = message };

            try
            {
                result.Message = _serializer.Deserialize<T>(message.MessageText);

                await _messageHandler(result.Message, stoppingToken);

                // ReSharper disable once MethodSupportsCancellation - Don't cancel a delete!
                await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error handling queued message of type {typeof(T).Name} with MessageId: {message.MessageId}");

                result.Exception = e;

                try
                {
                    if (message.DequeueCount >= 3)
                    {
                        var deadLetterQueueMessage = _deadLetterSerializer.Serialize(new DeadLetterQueueMessage
                        {
                            Text = message.MessageText,
                            OriginatingQueue = queue.Name,
                            Exception = e.InnermostException().To(x => x.Message + Environment.NewLine + x.StackTrace)
                        });

                        await deadLetterQueue.SendMessageAsync(deadLetterQueueMessage, stoppingToken);

                        result.WasDeadLettered = true;
                    }
                    else
                    {
                        // ReSharper disable once MethodSupportsCancellation - Don't cancel a re-enqueue!
                        await queue.UpdateMessageAsync(message.MessageId, message.PopReceipt, message.MessageText, TimeSpan.Zero);

                        result.WasAbandoned = true;
                    }
                }
                catch (Exception secondaryException)
                {
                    result.SecondaryException = secondaryException;

                    _logger.LogError(secondaryException, $"Error on recovering from queued message failure of type {typeof(T).Name} with MessageId: {message.MessageId}, DequeueCount: {message.DequeueCount}");
                }
            }

            return result;
        }
    }

    public class DeadLetterQueueMessage
    {
        public string Text { get; set; }
        public string OriginatingQueue { get; set; }
        public string Exception { get; set; }
    }
}
