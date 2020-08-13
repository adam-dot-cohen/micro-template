using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Laso.Catalog.Domain.FileSchema.Input;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class HttpClassifyBatch
    {
        private readonly IAccountTransactionClassifier _classifier;
        private readonly ILogger<HttpClassifyBatch> _logger;

        public HttpClassifyBatch(IAccountTransactionClassifier classifier, ILogger<HttpClassifyBatch> logger)
        {
            _classifier = classifier;
            _logger = logger;
        } 

        [FunctionName("HttpClassifyBatch")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("HttpClassifyBatch function processing a request.");

            _logger.LogDebug("Reading transactions.");
            var transactions = await JsonSerializer
                .DeserializeAsync<IEnumerable<AccountTransaction_v0_3>>(request.Body, cancellationToken: cancellationToken);

            _logger.LogDebug("Classifying transactions.");
            var classes = await _classifier.Classify(transactions, cancellationToken);

            return new OkObjectResult(classes);
        }
    }
}
