using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class AzureServiceBusTopicProvider
    {
        private readonly string _connectionString;
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusTopicProvider(AzureServiceBusConfiguration configuration, string connectionString)
        {
            _connectionString = connectionString;
            _configuration = configuration;
        }

        internal async Task<TopicClient> GetTopicClient(Type eventType, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(eventType), cancellationToken);

            return new TopicClient(_connectionString, topic.Path);
        }

        internal async Task<SubscriptionClient> GetSubscriptionClient(Type eventType, string subscriptionName, string sqlFilter = null, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            SubscriptionDescription subscription;

            var newFilter = new SqlFilter(sqlFilter ?? "1=1");

            if (!await managementClient.SubscriptionExistsAsync(topic.Path, subscriptionName, cancellationToken))
            {
                subscription = await managementClient.CreateSubscriptionAsync(
                    new SubscriptionDescription(topic.Path, subscriptionName),
                    new RuleDescription(RuleDescription.DefaultRuleName, newFilter),
                    cancellationToken);

                return new SubscriptionClient(_connectionString, subscription.TopicPath, subscription.SubscriptionName);
            }

            subscription = await managementClient.GetSubscriptionAsync(topic.Path, subscriptionName, cancellationToken);

            var subscriptionClient = new SubscriptionClient(_connectionString, subscription.TopicPath, subscription.SubscriptionName);

            var rules = await subscriptionClient.GetRulesAsync();
            var oldFilter = rules.FirstOrDefault(x => x.Name == RuleDescription.DefaultRuleName)?.Filter as SqlFilter;

            if (oldFilter == null)
            {
                await subscriptionClient.AddRuleAsync(new RuleDescription(RuleDescription.DefaultRuleName, newFilter));
            }
            else if (oldFilter.SqlExpression != newFilter.SqlExpression)
            {
                await subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName);

                await subscriptionClient.AddRuleAsync(new RuleDescription(RuleDescription.DefaultRuleName, newFilter));
            }

            return subscriptionClient;
        }

        protected virtual async Task<TopicDescription> GetTopicDescription(ManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
        {
            var topicName = GetTopicName(eventType);

            return await managementClient.TopicExistsAsync(topicName, cancellationToken)
                ? await managementClient.GetTopicAsync(topicName, cancellationToken)
                : await managementClient.CreateTopicAsync(topicName, cancellationToken);
        }

        protected string GetTopicName(Type eventType)
        {
            var eventTypeName = eventType.Name;
            var index = eventTypeName.IndexOf('`');

            if (index > 0)
                eventTypeName = eventTypeName.Substring(0, index);

            var name = _configuration.TopicNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{EventName}", eventTypeName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }
    }
}