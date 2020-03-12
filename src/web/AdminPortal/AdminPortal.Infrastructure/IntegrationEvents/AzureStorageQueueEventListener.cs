using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueEventListener<T> : BackgroundService
    {
        private readonly AzureStorageQueueProvider _queueProvider;
        private readonly Func<T, Task> _eventHandler;
        private readonly TimeSpan? _pollingDelay;
        private readonly TimeSpan? _visibilityTimeout;

        public AzureStorageQueueEventListener(AzureStorageQueueProvider queueProvider, Func<T, Task> eventHandler, TimeSpan? pollingDelay = null, TimeSpan? visibilityTimeout = null)
        {
            _queueProvider = queueProvider;
            _eventHandler = eventHandler;
            _pollingDelay = pollingDelay;
            _visibilityTimeout = visibilityTimeout;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            QueueClient queue = null;
            QueueClient deadLetterQueue = null;

            while (!stoppingToken.IsCancellationRequested && queue == null)
            {
                try
                {
                    queue = await _queueProvider.GetQueue(typeof(T), stoppingToken);
                }
                catch (Exception)
                {
                    //TODO: logging

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await queue.ReceiveMessagesAsync(visibilityTimeout: _visibilityTimeout, cancellationToken: stoppingToken);

                    await Task.WhenAll(messages.Value.Select(async x =>
                    {
                        try
                        {
                            var @event = JsonSerializer.Deserialize<T>(x.MessageText);

                            await _eventHandler(@event);

                            // ReSharper disable once MethodSupportsCancellation - Don't cancel a delete!
                            await queue.DeleteMessageAsync(x.MessageId, x.PopReceipt);
                        }
                        catch (Exception)
                        {
                            //TODO: logging?

                            try
                            {
                                if (x.DequeueCount >= 3)
                                {
                                    if (deadLetterQueue == null)
                                        deadLetterQueue = await _queueProvider.GetDeadLetterQueue(stoppingToken);

                                    await deadLetterQueue.SendMessageAsync(x.MessageText, stoppingToken);
                                }
                                else
                                {
                                    // ReSharper disable once MethodSupportsCancellation - Don't cancel a re-enqueue!
                                    await queue.UpdateMessageAsync(x.MessageId, x.PopReceipt, visibilityTimeout: TimeSpan.Zero);
                                }
                            }
                            catch (Exception)
                            {
                                //TODO: logging
                            }
                        }
                    }));
                }
                catch (Exception)
                {
                    //TODO: logging
                }
                finally
                {
                    await Task.Delay(_pollingDelay ?? TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
