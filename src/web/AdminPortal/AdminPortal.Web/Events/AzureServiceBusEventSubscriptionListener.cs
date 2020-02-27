using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.AdminPortal.Web.Events
{
    public class AzureServiceBusEventSubscriptionListener<T> : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _subscriptionName;

        private SubscriptionClient _client;

        public AzureServiceBusEventSubscriptionListener(string connectionString, string subscriptionName)
        {
            _connectionString = connectionString;
            _subscriptionName = subscriptionName;
        }

        public async Task Open(Action<T> onEvent)
        {
            if (onEvent == null)
                throw new ArgumentNullException(nameof(onEvent));

            while (_client == null)
            {
                try
                {
                    var managementClient = new ManagementClient(_connectionString);

                    var topicName = typeof(T).Name.ToLower();

                    var topic = await managementClient.TopicExistsAsync(topicName)
                        ? await managementClient.GetTopicAsync(topicName)
                        : await managementClient.CreateTopicAsync(topicName);

                    var subscription = await managementClient.SubscriptionExistsAsync(topic.Path, _subscriptionName)
                        ? await managementClient.GetSubscriptionAsync(topic.Path, _subscriptionName)
                        : await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Path, _subscriptionName));

                    var options = new MessageHandlerOptions(async x =>
                    {
                        //TODO: logging

                        if (_client != null)
                        {
                            await Close();

                            await Open(onEvent);
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

                            onEvent(@event);

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

                    await Task.Delay(TimeSpan.FromSeconds(10));
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

        public void Dispose()
        {
            Task.WaitAll(Close());
        }
    }
}
