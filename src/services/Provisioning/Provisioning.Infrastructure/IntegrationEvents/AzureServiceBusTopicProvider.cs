using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.Provisioning.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusTopicProvider
    {
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusTopicProvider(string connectionString, string topicNameFormat)
        {
            _configuration = new AzureServiceBusConfiguration
            {
                ConnectionString = connectionString,
                TopicNameFormat = topicNameFormat
            };
        }

        public async Task<TopicClient> GetTopicClient(Type eventType, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_configuration.ConnectionString);

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            return new TopicClient(_configuration.ConnectionString, topic.Path);
        }

        public async Task<SubscriptionClient> GetSubscriptionClient(Type eventType, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_configuration.ConnectionString);

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            var subscription = await managementClient.SubscriptionExistsAsync(topic.Path, subscriptionName, cancellationToken)
                ? await managementClient.GetSubscriptionAsync(topic.Path, subscriptionName, cancellationToken)
                : await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Path, subscriptionName), cancellationToken);

            return new SubscriptionClient(_configuration.ConnectionString, subscription.TopicPath, subscription.SubscriptionName);
        }

        protected virtual async Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            var topicName = GetTopicName(eventType);

            return await managementClient.TopicExistsAsync(topicName, cancellationToken)
                ? await managementClient.GetTopicAsync(topicName, cancellationToken)
                : await managementClient.CreateTopicAsync(topicName, cancellationToken);
        }

        private string GetTopicName(Type eventType)
        {
            var name = _configuration.TopicNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{EventName}", eventType.Name);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }
    }

    public class AzureServiceBusConfiguration
    {
        public string ConnectionString { get; set; }
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}