using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusTopicProvider
    {
        private readonly string _connectionString;
        private readonly AzureServiceBusConfiguration _configuration;

        private readonly HashSet<string> _createdTopics = new HashSet<string>();

        public AzureServiceBusTopicProvider(string connectionString, AzureServiceBusConfiguration configuration)
        {
            _connectionString = connectionString;
            _configuration = configuration;
        }

        public async Task<TopicClient> GetTopicClient(Type eventType, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            return new TopicClient(_connectionString, topic.Path);
        }

        public async Task<SubscriptionClient> GetSubscriptionClient(Type eventType, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            var subscription = await managementClient.SubscriptionExistsAsync(topic.Path, subscriptionName, cancellationToken)
                ? await managementClient.GetSubscriptionAsync(topic.Path, subscriptionName, cancellationToken)
                : await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Path, subscriptionName), cancellationToken);

            return new SubscriptionClient(_connectionString, subscription.TopicPath, subscription.SubscriptionName);
        }

        private async Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            var topicName = GetTopicName(eventType);

            TopicDescription topic;

            if (await managementClient.TopicExistsAsync(topicName, cancellationToken))
            {
                topic = await managementClient.GetTopicAsync(topicName, cancellationToken);
            }
            else
            {
                topic = await managementClient.CreateTopicAsync(topicName, cancellationToken);

                lock (_createdTopics)
                {
                    _createdTopics.Add(topicName);
                }
            }

            return topic;
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

        public async Task DeleteCreatedTopics()
        {
            var managementClient = new ManagementClient(_connectionString);
            var tasks = new List<Task>();

            lock (_createdTopics)
            {
                foreach (var topic in _createdTopics)
                    tasks.Add(managementClient.DeleteTopicAsync(topic));

                _createdTopics.Clear();
            }

            await Task.WhenAll(tasks);
        }
    }

    public class AzureServiceBusConfiguration
    {
        public string EndpointUrl { get; set; }
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}