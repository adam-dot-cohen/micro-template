using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class AzureServiceBusClassifyBatch
    {
        private readonly IAccountTransactionClassifyBatchProcess _process;
        private readonly ILogger<AzureServiceBusClassifyBatch> _logger;

        public AzureServiceBusClassifyBatch(IAccountTransactionClassifyBatchProcess process, ILogger<AzureServiceBusClassifyBatch> logger)
        {
            _process = process;
            _logger = logger;
        }

        [FunctionName("AzureServiceBusClassifyBatch")]
        public Task Run(
            [ServiceBusTrigger("scheduling", "AcctTxnClassifier.Function")] string msg,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"AzureServiceBusClassifyBatch topic trigger function processing message: {msg}");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var message = JsonSerializer.Deserialize<ExperimentRunScheduledEvent>(msg, options);
            var evt = message.Data;

            var now = DateTime.UtcNow;

            return _process.Run(
                evt.PartnerId, 
                Path.GetFileName(evt.Files.First().Uri),
                $"Laso_QuarterSpot_R_AccountTransactionClass_{now:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv",
                cancellationToken);
        }
    }
}
