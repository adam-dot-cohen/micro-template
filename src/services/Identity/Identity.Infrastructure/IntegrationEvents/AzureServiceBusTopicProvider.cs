using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusTopicProvider
    {
        private readonly string _connectionString;
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusTopicProvider(string connectionString, AzureServiceBusConfiguration configuration)
        {
            _connectionString = connectionString;
            _configuration = configuration;
        }

        public async Task<TopicClient> GetTopicClient(Type eventType, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(eventType), cancellationToken);

            return new TopicClient(_connectionString, topic.Path);
        }

        public async Task<SubscriptionClient> GetSubscriptionClient(Type eventType, string subscriptionName, string sqlFilter, CancellationToken cancellationToken = default)
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

        public async Task<MessageReceiver> GetDeadLetterClient(Type eventType, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(eventType), cancellationToken);

            var path = EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topic.Path, subscriptionName));

            return new MessageReceiver(_connectionString, path);
        }

        public async Task<string> GetSqlFilter(Type eventType, string subscriptionName, CancellationToken cancellationToken = default)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topic = await managementClient.GetTopicAsync(GetTopicName(eventType), cancellationToken);

            var subscription = await managementClient.GetSubscriptionAsync(topic.Path, subscriptionName, cancellationToken);

            var subscriptionClient = new SubscriptionClient(_connectionString, subscription.TopicPath, subscription.SubscriptionName);

            var rules = await subscriptionClient.GetRulesAsync();

            return (rules.FirstOrDefault(x => x.Name == RuleDescription.DefaultRuleName)?.Filter as SqlFilter)?.SqlExpression;
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

    public class AzureServiceBusConfiguration
    {
        public string EndpointUrl { get; set; }
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}