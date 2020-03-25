using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
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

                    await Task.WhenAll(messages.Value.Select(async x =>
                    {
                        try
                        {
                            var @event = _messageDeserializer(x.MessageText);

                            await _eventHandler(@event, stoppingToken);

                            // ReSharper disable once MethodSupportsCancellation - Don't cancel a delete!
                            await queue.DeleteMessageAsync(x.MessageId, x.PopReceipt);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Error handling queued message of type {typeof(T).Name} with MessageId: {x.MessageId}");

                            try
                            {
                                if (x.DequeueCount >= 3)
                                {
                                    await deadLetterQueue.SendMessageAsync(x.MessageText, stoppingToken);
                                }
                                else
                                {
                                    // ReSharper disable once MethodSupportsCancellation - Don't cancel a re-enqueue!
                                    await queue.UpdateMessageAsync(x.MessageId, x.PopReceipt, x.MessageText, TimeSpan.Zero);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error on recovering from queued message failure of type {typeof(T).Name} with MessageId: {x.MessageId}, DequeueCount: {x.DequeueCount}");
                            }
                        }
                    }));
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
    }
}
