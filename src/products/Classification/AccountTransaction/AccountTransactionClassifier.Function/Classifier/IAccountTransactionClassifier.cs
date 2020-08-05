using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Laso.Catalog.Domain.FileSchema;

// ReSharper disable InconsistentNaming

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public interface IAccountTransactionClassifier
    {
        Task<IEnumerable<TransactionClass>> Classify(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken);
    }

    public class TransactionClass
    {
        public string Transaction_Id { get; set; } = null!;
        public long? Class { get; set; }
    }
}
