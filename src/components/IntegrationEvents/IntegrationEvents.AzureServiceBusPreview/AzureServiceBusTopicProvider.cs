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

        public AzureServiceBusTopicProvider(string connectionString, AzureServiceBusConfiguration configuration)
        {
            _connectionString = connectionString;
            _configuration = configuration;
        }

        internal ServiceBusSender GetSender(Type eventType)
        {
            var client = string.IsNullOrWhiteSpace(_configuration.ServiceUrl)
                ? new ServiceBusClient(_connectionString)
                : new ServiceBusClient(_configuration.ServiceUrl.Trim(), new DefaultAzureCredential());

            return client.CreateSender(GetTopicName(eventType));
        }

        internal async Task<Func<ServiceBusProcessor>> GetProcessorFactory(Type eventType, string subscriptionName, string sqlFilter, ServiceBusProcessorOptions serviceBusProcessorOptions, CancellationToken cancellationToken)
        {
            var managementClient = string.IsNullOrWhiteSpace(_configuration.ServiceUrl)
                ? new ServiceBusManagementClient(_connectionString)
                : new ServiceBusManagementClient(_configuration.ServiceUrl.Trim(), new DefaultAzureCredential());

            var topic = await GetTopicDescription(managementClient, eventType, cancellationToken);

            var newFilter = new SqlRuleFilter(sqlFilter ?? "1=1");
            var rule = new RuleDescription(RuleDescription.DefaultRuleName, newFilter);

            if (await managementClient.SubscriptionExistsAsync(topic.Name, subscriptionName, cancellationToken))
            {
                var rules = managementClient.GetRulesAsync(topic.Name, subscriptionName, cancellationToken).ToEnumerable();

                var oldFilter = (await rules.FirstOrDefaultAsync(x => x.Name == RuleDescription.DefaultRuleName, cancellationToken))?.Filter as SqlRuleFilter;

                if (oldFilter == null)
                    await managementClient.CreateRuleAsync(topic.Name, subscriptionName, rule, cancellationToken);
                else if (oldFilter.SqlExpression != newFilter.SqlExpression)
                    await managementClient.UpdateRuleAsync(topic.Name, subscriptionName, rule, cancellationToken);
            }
            else
            {
                await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topic.Name, subscriptionName), rule, cancellationToken);
            }

            var client = string.IsNullOrWhiteSpace(_configuration.ServiceUrl)
                ? new ServiceBusClient(_connectionString)
                : new ServiceBusClient(_configuration.ServiceUrl.Trim(), new DefaultAzureCredential());

            return () => client.CreateProcessor(topic.Name, subscriptionName, serviceBusProcessorOptions);
        }

        protected virtual async Task<TopicDescription> GetTopicDescription(ServiceBusManagementClient managementClient, Type eventType, CancellationToken cancellationToken)
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

    public class AzureServiceBusConfiguration
    {
        public string ServiceUrl { get; set; }
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}