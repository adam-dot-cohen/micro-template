using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Insights.AccountTransactionClassifier.Function
{
    public static class AzureServiceBusClassifyBatch
    {
        private const string QuarterSpotPartnerId = "6c34c5bb-b083-4e62-a83e-cb0532754809";

        [FunctionName(nameof(AzureServiceBusClassifyBatch))]
        public static Task RunAsync(
            [ServiceBusTrigger("mytopic", "ClassifyBatch", Connection = "AzureServiceBus")] string message,
            ExecutionContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var configuration = GetConfiguration(context.FunctionAppDirectory);
            const string filename = "incoming/QuarterSpot_Laso_R_AccountTransaction_v0.3_20200803_20200803181652.csv";

            var classifyJob = new ClassifyBatch();
            return classifyJob.Run(QuarterSpotPartnerId, filename, configuration, logger, cancellationToken);
        }
        
        private static IConfiguration GetConfiguration(string workingDirectory)
        {
            return new ConfigurationBuilder()
                .SetBasePath(workingDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
