using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class AzureServiceBusClassifyBatch
    {
        private readonly ILogger<AzureServiceBusClassifyBatch> _logger;

        public AzureServiceBusClassifyBatch(ILogger<AzureServiceBusClassifyBatch> logger)
        {
            _logger = logger;
        }

        [FunctionName("AzureServiceBusClassifyBatch")]
        public void Run([ServiceBusTrigger("scheduling", "AcctTxnClassifier.Function")] string message)
        {
            _logger.LogInformation($"AzureServiceBusClassifyBatch topic trigger function processing message: {message}");
        }
    }
}
