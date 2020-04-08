using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Infrastructure.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Laso.Provisioning.Api.IntegrationEvents
{
    public class AzureServiceBusSubscriptionEventListener<T> : BackgroundService
    {
        private readonly ILogger<AzureServiceBusSubscriptionEventListener<T>> _logger;
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Func<T, Task> _eventHandler;

        private SubscriptionClient _client;

        public AzureServiceBusSubscriptionEventListener(
            ILogger<AzureServiceBusSubscriptionEventListener<T>> logger,
            AzureServiceBusTopicProvider topicProvider, 
            string subscriptionName,
            Func<T, Task> eventHandler)
        {
            _logger = logger;
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _eventHandler = eventHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Open(stoppingToken);
        }

        public async Task Open(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Task.WaitAll(Close()));

            while (!stoppingToken.IsCancellationRequested && _client == null)
            {
                try
                {
                    var client = await _topicProvider.GetSubscriptionClient(typeof(T), _subscriptionName, stoppingToken);

                    var options = new MessageHandlerOptions(async x =>
                    {
                        _logger.LogCritical(x.Exception,"Exception during message handling.");

                        if (_client != null)
                        {
                            await Close();

                            await Open(stoppingToken);
                        }
                    }) { AutoComplete = false };

                    client.RegisterMessageHandler(
                        async (x, y) =>
                        {
                            if (client.IsClosedOrClosing)
                                return;

                            try
                            {
                                var @event = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(x.Body));

                                await _eventHandler(@event);

                                await client.CompleteAsync(x.SystemProperties.LockToken);
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical(e, "Exception attempting to handle message.");

                                try
                                {
                                    if (x.SystemProperties.DeliveryCount >= 3)
                                        await client.DeadLetterAsync(x.SystemProperties.LockToken, "Exceeded retries");
                                }
                                catch (Exception deadLetterException)
                                {
                                    _logger.LogCritical(deadLetterException, "Exception attempting to dead letter message.");
                                }
                            }
                        },
                        options);

                    _client = client;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception attempting to configure topic subscription.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task Close()
        {
            if (_client == null || _client.IsClosedOrClosing)
            {
                _client = null;

                return;
            }

            try
            {
                await _client.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception attempting close event listener.");
            }

            _client = null;
        }
    }
}
