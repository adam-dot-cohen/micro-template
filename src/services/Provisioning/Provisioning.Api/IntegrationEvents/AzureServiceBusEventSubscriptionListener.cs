using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Hosting;

namespace Laso.Provisioning.Api.IntegrationEvents
{
    public class AzureServiceBusEventSubscriptionListener<T> : BackgroundService
    {
        private readonly string _connectionString;
        private readonly string _subscriptionName;
        private readonly Action<T> _eventHandler;

        private SubscriptionClient _client;

        public AzureServiceBusEventSubscriptionListener(string connectionString, string subscriptionName, Action<T> eventHandler)
        {
            _connectionString = connectionString;
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
                    var managementClient = new ManagementClient(_connectionString);

                    var topicName = typeof(T).Name.ToLower();

                    var topic = await managementClient.TopicExistsAsync(topicName, stoppingToken)
                        ? await managementClient.GetTopicAsync(topicName, stoppingToken)
                        : await managementClient.CreateTopicAsync(topicName, stoppingToken);

                    var subscription = await managementClient.SubscriptionExistsAsync(topic.Path, _subscriptionName, stoppingToken)
                        ? await managementClient.GetSubscriptionAsync(topic.Path, _subscriptionName, stoppingToken)
                        : await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Path, _subscriptionName), stoppingToken);

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

                    var client = new SubscriptionClient(_connectionString, subscription.TopicPath, subscription.SubscriptionName);

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
                        catch (Exception e)
                        {
                            //TODO: logging
                            Debug.WriteLine(e.Message);

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

        public async Task Close()
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
