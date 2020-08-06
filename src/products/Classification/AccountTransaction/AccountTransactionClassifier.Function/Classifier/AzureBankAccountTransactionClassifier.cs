﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Laso.Catalog.Domain.FileSchema;

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public class AzureBankAccountTransactionClassifier : IAccountTransactionClassifier
    {
        private readonly IAccountTransactionNormalizer _normalizer;
        private readonly IMachineLearningService _machineLearningService;

        public AzureBankAccountTransactionClassifier(
            IAccountTransactionNormalizer normalizer, IMachineLearningService machineLearningService)
        {
            _normalizer = normalizer;
            _machineLearningService = machineLearningService;
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
            var input = new MachineLearningExecutionObject
            {
                { "input1", transactions.ToDictionary(t => "NormalizedText", t => (object?)t.Memo_Field) }
            };

            //_machineLearningService.Execute(input);

            //        calculationModel.MachineLearningConfiguration.AzureMlEndpoint,
            //        calculationModel.MachineLearningConfiguration.AzureMlApiKey,

            return Task.FromResult(Enumerable.Empty<TransactionClass>());
        }

        private static Task<IEnumerable<TransactionClass>> ClassifyDebits(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken)
        {
            IEnumerable<TransactionClass> classes = transactions
                .Select(t => new TransactionClass {Transaction_Id = t.Transaction_Id})
                .ToList();

            return Task.FromResult(classes);
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

        //public MachineLearningExecutionObject GetInput(AccountTransaction_v0_3 transaction, BankTransactionClassifierCalculationModel calculationModel)
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
