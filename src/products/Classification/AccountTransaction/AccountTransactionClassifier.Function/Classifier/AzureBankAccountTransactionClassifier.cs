using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Catalog.Domain.FileSchema;

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public class AzureBankAccountTransactionClassifier : IAccountTransactionClassifier
    {
        public async Task<IEnumerable<long>> Classify(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            var transactionsList = transactions.ToList();

            var classifyTasks = new[]
            {
                ClassifyCredits(transactionsList.Where(t => t.Amount >= 0), cancellationToken),
                ClassifyDebits(transactionsList.Where(t => t.Amount < 0), cancellationToken)
            };

            var classifyResults = await Task.WhenAll(classifyTasks);

            // TODO: Make sure all tasks completed successfully.

            var response = classifyResults
                .SelectMany(r => r)
                .ToList();

            return response;
        }

        private static Task<IEnumerable<long>> ClassifyCredits(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<long>().AsEnumerable());
        }

        private static Task<IEnumerable<long>> ClassifyDebits(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<long>().AsEnumerable());
        }

        //private Task Classify(ICollection<AccountTransaction_v0_3> transactions)
        //{
        //    if (!transactions.Any())
        //        return Task.CompletedTask;

        //    var normalizer = _factory.GetInstance<IBankTransactionNormalizer>(calculationModel.NormalizerType);

        //    var inputs = transactions
        //        .Select(x => GetInput(x.Map(normalizer), calculationModel))
        //        .ToCollection();

        //    var outputs = _machineLearningService.Execute(
        //        calculationModel.MachineLearningConfiguration.AzureMlEndpoint,
        //        calculationModel.MachineLearningConfiguration.AzureMlApiKey,
        //        inputs);

        //    transactions
        //        .Zip(outputs, (x, y) => new
        //        {
        //            Transaction = x,
        //            Category = BankAccountTransactionCategory.FromValue(y[calculationModel.Output.ParentObject.Name][calculationModel.Output.ColumnName].ToInt32())
        //        })
        //        .ForEach(x =>
        //        {
        //            x.Transaction.Category = x.Category;
        //            x.Transaction.AutoCategorized = true;
        //        });
        //}

        //public MachineLearningExecutionObject GetInput(IBankAccountTransaction transaction, BankTransactionClassifierCalculationModel calculationModel)
        //{
        //    var mappings = calculationModel.InputColumns.ToDictionary(x => x.ColumnMapping, x => BankAccountTransactionMapper.GetAllProperties().First(y => y.Name == x.PropertyName));

        //    return new MachineLearningExecutionObject().With(x => calculationModel.MachineLearningConfiguration.Inputs
        //        .ForEach(y => x.Add(y.Name, y.Columns.ToDictionary(z => z.ColumnName, z =>
        //        {
        //            var value = mappings[z].GetValue(transaction);

        //            return value == null ? null : z.CoerceAndNormalize(value);
        //        }))));
        //}
    }
}
