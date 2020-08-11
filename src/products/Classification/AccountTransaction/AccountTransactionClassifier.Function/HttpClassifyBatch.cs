using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Laso.Catalog.Domain.FileSchema.Input;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Insights.AccountTransactionClassifier.Function
{
    public static class HttpClassifyBatch
    {
        [FunctionName("HttpClassifyBatch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            ExecutionContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("HttpClassifyBatch function processing a request.");
            var configuration = Configuration.GetConfiguration(context.FunctionAppDirectory);

            logger.LogDebug("Reading transactions.");
            var transactions = await JsonSerializer
                .DeserializeAsync<IEnumerable<AccountTransaction_v0_3>>(request.Body, cancellationToken: cancellationToken);

            logger.LogDebug("Classifying transactions.");
            var normalizer = new AccountTransactionNormalizer();
            var creditsMachineLearningService = new AzureMachineLearningService(new RetryPolicy())
            {
                BaseUrl = configuration["Components:AzureCreditsBankTransactionClassifier:Endpoint"],
                ApiKey = configuration["Components:AzureCreditsBankTransactionClassifier:Key"]
            };
            var debitsMachineLearningService = new AzureMachineLearningService(new RetryPolicy())
            {
                BaseUrl = configuration["Components:AzureDebitsBankTransactionClassifier:Endpoint"],
                ApiKey = configuration["Components:AzureDebitsBankTransactionClassifier:Key"]
            };
            var classifier = new AzureBankAccountTransactionClassifier(normalizer, creditsMachineLearningService, debitsMachineLearningService);
            var classes = await classifier.Classify(transactions, cancellationToken);

            return new OkObjectResult(classes);
        }
    }
}
