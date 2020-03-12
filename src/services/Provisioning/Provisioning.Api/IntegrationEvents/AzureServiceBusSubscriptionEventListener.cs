using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.Infrastructure.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Laso.Provisioning.Api.IntegrationEvents
{
    public class AzureServiceBusSubscriptionEventListener<T> : BackgroundService
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Func<T, Task> _eventHandler;

        private SubscriptionClient _client;

        public AzureServiceBusSubscriptionEventListener(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, Task> eventHandler)
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

                            await _eventHandler(@event);

                            await client.CompleteAsync(x.SystemProperties.LockToken);
                        }
                        catch (Exception e)
                        {
                            //TODO: logging?
                            Debug.WriteLine(e.Message);

                            try
                            {
                                if (x.SystemProperties.DeliveryCount >= 3)
                                    await client.DeadLetterAsync(x.SystemProperties.LockToken, "Exceeded retries");
                            }
                            catch (Exception)
                            {
                                //TODO: logging
                            }
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
