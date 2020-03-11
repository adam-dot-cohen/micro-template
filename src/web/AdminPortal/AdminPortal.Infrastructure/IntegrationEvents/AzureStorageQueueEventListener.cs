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
                    queue = await _queueProvider.GetQueue(typeof(T));
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

                            await queue.DeleteMessageAsync(x.MessageId, x.PopReceipt, stoppingToken);
                        }
                        catch (Exception)
                        {
                            //TODO: logging

                            if (x.DequeueCount >= 3)
                            {
                                if (deadLetterQueue == null)
                                    deadLetterQueue = await _queueProvider.GetDeadLetterQueue();

                                await deadLetterQueue.SendMessageAsync(x.MessageText, stoppingToken);
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
