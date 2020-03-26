using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueEventListener<T> : IEventListener
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly Func<T, CancellationToken, Task> _eventHandler;
        private readonly ILogger<AzureStorageQueueEventListener<T>> _logger;
        private readonly TimeSpan? _pollingDelay;
        private readonly TimeSpan? _visibilityTimeout;
        private readonly Func<string, T> _messageDeserializer;

        private Task _pollingTask;

        public AzureStorageQueueEventListener(
            AzureStorageQueueProvider queueProvider,
            Func<T, CancellationToken, Task> eventHandler,
            ILogger<AzureStorageQueueEventListener<T>> logger = null,
            TimeSpan? pollingDelay = null,
            TimeSpan? visibilityTimeout = null,
            Func<string, T> messageDeserializer = null)
        {
            _queueProvider = queueProvider;
            _eventHandler = eventHandler;
            _logger = logger ?? new NullLogger<AzureStorageQueueEventListener<T>>();
            _pollingDelay = pollingDelay;
            _visibilityTimeout = visibilityTimeout;
            _messageDeserializer = messageDeserializer ?? DeserializeMessage;
        }

        private static T DeserializeMessage(string messageText)
        {
            var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
            var message = JsonSerializer.Deserialize<T>(messageText, options);

            return message;
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
                    deadLetterQueue = _queueProvider.GetDeadLetterQueue(stoppingToken);

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

                    await Task.WhenAll(messages.Value.Select(x => ProcessEvent(queue, deadLetterQueue, x, stoppingToken)));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error receiving queued message of type {typeof(T).Name}");
                }
                finally
                {
                    await Task.Delay(_pollingDelay ?? TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        protected virtual async Task<EventProcessingResult<QueueMessage, T>> ProcessEvent(QueueClient queue, QueueClient deadLetterQueue, QueueMessage message, CancellationToken stoppingToken)
        {
            var result = new EventProcessingResult<QueueMessage, T> { Message = message };

            try
            {
                result.DeserializedMessage = _messageDeserializer(message.MessageText);

                await _eventHandler(result.DeserializedMessage, stoppingToken);

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
                        result.WasDeadLettered = true;

                        await deadLetterQueue.SendMessageAsync(message.MessageText, stoppingToken);
                    }
                    else
                    {
                        result.WasAbandoned = true;

                        // ReSharper disable once MethodSupportsCancellation - Don't cancel a re-enqueue!
                        await queue.UpdateMessageAsync(message.MessageId, message.PopReceipt, message.MessageText, TimeSpan.Zero);
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
}
