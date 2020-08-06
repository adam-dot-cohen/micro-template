using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Extensions;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Laso.Catalog.Domain.FileSchema;

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public class AzureBankAccountTransactionClassifier : IAccountTransactionClassifier
    {
        private readonly IAccountTransactionNormalizer _normalizer;
        private readonly IMachineLearningService _creditsMachineLearningService;
        private readonly IMachineLearningService _debitsMachineLearningService;

        public AzureBankAccountTransactionClassifier(
            IAccountTransactionNormalizer normalizer, 
            IMachineLearningService creditsMachineLearningService,
            IMachineLearningService debitsMachineLearningService)
        {
            _normalizer = normalizer;
            _creditsMachineLearningService = creditsMachineLearningService;
            _debitsMachineLearningService = debitsMachineLearningService;
        }

        public async Task<IEnumerable<TransactionClass>> Classify(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            var transactionsList = transactions.ToList();

            var classifyTasks = new[]
            {
                ClassifyCredits(transactionsList.Where(t => t.Amount >= 0), cancellationToken),
                ClassifyDebits(transactionsList.Where(t => t.Amount < 0), cancellationToken)
            };

            var classifyResults = await Task.WhenAll(classifyTasks);

            // TODO: Check all tasks completed successfully.

            var response = classifyResults
                .SelectMany(r => r)
                .ToList();

            return response;
        }

        private Task<IEnumerable<TransactionClass>> ClassifyCredits(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            return Classify(transactions.ToList(), _creditsMachineLearningService, cancellationToken);
        }

        private Task<IEnumerable<TransactionClass>> ClassifyDebits(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            return Classify(transactions.ToList(), _debitsMachineLearningService, cancellationToken);
        }

        private async Task<IEnumerable<TransactionClass>> Classify(
            IList<AccountTransaction_v0_3> transactions, IMachineLearningService machineLearningService, CancellationToken cancellationToken)
        {
            var inputs = transactions
                .Select(t => new MachineLearningExecutionObject
                {
                    ["input1"] = new Dictionary<string, object?> { ["NormalizedText"] = _normalizer.NormalizeTransactionText(t) }
                });

            var response = await machineLearningService.Execute(inputs, cancellationToken);

            var results = response
                .Select((r, i) => new TransactionClass
                {
                    Transaction_Id = transactions[i].Transaction_Id,
                    Class = r["output1"]["Scored Labels"].ConvertTo<long>()
                })
                .ToList();

            return results;
        }
    }
}
