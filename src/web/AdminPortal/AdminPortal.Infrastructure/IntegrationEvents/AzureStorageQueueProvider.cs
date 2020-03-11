using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.AdminPortal.Core.Extensions;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueProvider
    {
        private readonly string _endpointUrl;
        private readonly string _queueNameFormat;

        private readonly ICollection<QueueClient> _createdQueues = new List<QueueClient>();

        public AzureStorageQueueProvider(string endpointUrl, string queueNameFormat = "{EventName}")
        {
            _endpointUrl = endpointUrl;
            _queueNameFormat = queueNameFormat;
        }

        public async Task<QueueClient> GetQueue(Type eventType)
        {
            return await GetQueue(GetQueueName(eventType.Name));
        }

        public async Task<QueueClient> GetDeadLetterQueue()
        {
            return await GetQueue(GetQueueName("DeadLetter"));
        }

        private async Task<QueueClient> GetQueue(string queueName)
        {
            var queueUri = new Uri(_endpointUrl.Trim().If(x => !x.EndsWith("/"), x => x + "/") + queueName);

            var client = new QueueClient(queueUri, new DefaultAzureCredential());

            try
            {
                await client.CreateAsync();

                lock (_createdQueues)
                {
                    _createdQueues.Add(client);
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == QueueErrorCode.QueueAlreadyExists) { } //TODO: change to CreateIfNotExistsAsync when available: https://github.com/Azure/azure-sdk-for-net/issues/7879

            return client;
        }

        private string GetQueueName(string queueName)
        {
            var name = _queueNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{EventName}", queueName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }

        public async Task DeleteCreatedQueues()
        {
            var tasks = new List<Task>();

            lock (_createdQueues)
            {
                foreach (var queue in _createdQueues)
                    tasks.Add(queue.DeleteAsync());

                _createdQueues.Clear();
            }

            await Task.WhenAll(tasks);
        }
    }
}