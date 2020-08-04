using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public static class ClassifyBatch
    {
        [FunctionName("ClassifyBatch")]
        public static Task RunAsync(
            [ServiceBusTrigger("mytopic", nameof(ClassifyBatch), Connection = "AzureServiceBus")] string message,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");

            var classifier = new AzureBankAccountTransactionClassifier();

            return Task.CompletedTask;
        }
    }
}
