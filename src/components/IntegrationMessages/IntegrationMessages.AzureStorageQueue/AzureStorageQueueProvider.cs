using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.IntegrationMessages.AzureStorageQueue.Extensions;

namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class AzureStorageQueueProvider
    {
        private const int MaxQueueNameLength = 63;

        private readonly string _connectionString;
        private readonly AzureStorageQueueConfiguration _configuration;

        public AzureStorageQueueProvider(AzureStorageQueueConfiguration configuration, string connectionString = null)
        {
            if ((configuration?.ServiceUrl ?? connectionString) == null)
                throw new ArgumentNullException();

            _connectionString = connectionString;
            _configuration = configuration;
        }

        internal async Task<QueueClient> GetQueue(Type messageType, CancellationToken cancellationToken = default)
        {
            if (messageType.Closes(typeof(IEnumerable<>), out var args))
                messageType = args.First()[0];

            return await GetQueue(messageType.Name, cancellationToken);
        }

        protected internal virtual async Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            var name = _configuration.QueueNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{MessageName}", queueName);

            name = new string(name.ToLower()
                    .Where(x => char.IsLetterOrDigit(x) || x == '-')
                    .SkipWhile(char.IsPunctuation)
                    .ToArray())
                .Truncate(MaxQueueNameLength);

            QueueClient client;

            if (string.IsNullOrWhiteSpace(_configuration.ServiceUrl))
            {
                client = new QueueClient(_connectionString, name);
            }
            else
            {
                var queueUri = new Uri(_configuration.ServiceUrl.Trim().If(x => !x.EndsWith("/"), x => x + "/") + name);

                client = new QueueClient(queueUri, new DefaultAzureCredential());
            }

            try
            {
                await client.CreateAsync(cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == QueueErrorCode.QueueAlreadyExists)
            {

            } //TODO: change to CreateIfNotExistsAsync when available: https://github.com/Azure/azure-sdk-for-net/issues/7879

            return client;
        }
    }
}