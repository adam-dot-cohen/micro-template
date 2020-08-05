using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Management;
using Laso.IntegrationEvents.AzureServiceBus.Preview.Extensions;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public class AzureServiceBusTopicProvider
    {
        private readonly string _connectionString;
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusTopicProvider(AzureServiceBusConfiguration configuration, string connectionString = null)
        {
            if ((configuration?.ServiceUrl ?? connectionString) == null)
                throw new ArgumentNullException();

            _connectionString = connectionString;
            _configuration = configuration;
        }

        internal ServiceBusSender GetSender(string topicName)
        {
            return GetClient().CreateSender(GetTopicName(topicName));
        }

        internal async Task<Func<ServiceBusProcessor>> GetProcessorFactory(string topicName, string subscriptionName, string sqlFilter, ServiceBusProcessorOptions serviceBusProcessorOptions, CancellationToken cancellationToken)
        {
            var managementClient = GetManagementClient();

            var topic = await GetTopicDescription(managementClient, topicName, cancellationToken);

            var newFilter = new SqlRuleFilter(sqlFilter ?? "1=1");
            var rule = new RuleDescription(RuleDescription.DefaultRuleName, newFilter);

            if (await managementClient.SubscriptionExistsAsync(topic.Name, subscriptionName, cancellationToken))
            {
                var ruleDescription = await managementClient.GetRulesAsync(topic.Name, subscriptionName, cancellationToken)
                    .ToEnumerable()
                    .FirstOrDefaultAsync(x => x.Name == RuleDescription.DefaultRuleName, cancellationToken);

                var oldFilter = ruleDescription?.Filter as SqlRuleFilter;

                if (ruleDescription == null)
                    await managementClient.CreateRuleAsync(topic.Name, subscriptionName, rule, cancellationToken);
                else if (oldFilter == null || oldFilter.SqlExpression != newFilter.SqlExpression)
                    await managementClient.UpdateRuleAsync(topic.Name, subscriptionName, rule, cancellationToken);
            }
            else
            {
                await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Name, subscriptionName), rule, cancellationToken);
            }

            return () => GetClient().CreateProcessor(topic.Name, subscriptionName, serviceBusProcessorOptions);
        }

        private ServiceBusClient GetClient()
        {
            return string.IsNullOrWhiteSpace(_configuration.ServiceUrl)
                ? new ServiceBusClient(_connectionString)
                : new ServiceBusClient(_configuration.ServiceUrl.Trim(), new DefaultAzureCredential());
        }

        private ServiceBusManagementClient GetManagementClient()
        {
            return string.IsNullOrWhiteSpace(_configuration.ServiceUrl)
                ? new ServiceBusManagementClient(_connectionString)
                : new ServiceBusManagementClient(_configuration.ServiceUrl.Trim(), new DefaultAzureCredential());
        }

        protected virtual async Task<TopicDescription> GetTopicDescription(ServiceBusManagementClient managementClient, string topicName, CancellationToken cancellationToken)
        {
            topicName = GetTopicName(topicName);

            return await managementClient.TopicExistsAsync(topicName, cancellationToken)
                ? await managementClient.GetTopicAsync(topicName, cancellationToken)
                : await managementClient.CreateTopicAsync(topicName, cancellationToken);
        }

        protected string GetTopicName(string topicName)
        {
            var index = topicName.IndexOf('`');

            if (index > 0)
                topicName = topicName.Substring(0, index);

            var name = _configuration.TopicNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{TopicName}", topicName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }
    }
}