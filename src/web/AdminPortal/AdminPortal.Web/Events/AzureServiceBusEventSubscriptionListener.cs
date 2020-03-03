using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.Web.Events
{
    public class AzureServiceBusEventSubscriptionListener<T> : BackgroundService
    {
        private readonly AzureTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Action<T> _eventHandler;

        private SubscriptionClient _client;

        public AzureServiceBusEventSubscriptionListener(AzureTopicProvider topicProvider, string subscriptionName, Action<T> eventHandler)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _eventHandler = eventHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Task.WaitAll(Close()));

            return Open(stoppingToken);
        }

        private async Task Open(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && _client == null)
            {
                try
                {
                    var client = await _topicProvider.GetSubscriptionClient(typeof(T), _subscriptionName, stoppingToken);

                    var options = new MessageHandlerOptions(async x =>
                    {
                        //TODO: logging

                        if (_client != null)
                        {
                            await Close();

                            await Open(stoppingToken);
                        }
                    })
                    {
                        AutoComplete = false,
                        MaxAutoRenewDuration = TimeSpan.FromMinutes(1),
                        MaxConcurrentCalls = 1
                    };

                    client.RegisterMessageHandler(async (x, y) =>
                    {
                        if (client.IsClosedOrClosing)
                            return;

                        try
                        {
                            var @event = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(x.Body));

                            _eventHandler(@event);

                            await client.CompleteAsync(x.SystemProperties.LockToken);
                        }
                        catch (Exception)
                        {
                            //TODO: logging

                            if (x.SystemProperties.DeliveryCount >= 3)
                                await client.DeadLetterAsync(x.SystemProperties.LockToken, "Exceeded retries");
                        }
                    }, options);

                    _client = client;
                }
                catch (Exception)
                {
                    //TODO: logging

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
            catch (Exception)
            {
                //TODO: logging
            }

            _client = null;
        }
    }
}
