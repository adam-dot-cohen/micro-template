using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Laso.Catalog.Domain.FileSchema;

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public interface IAccountTransactionClassifier
    {
        Task<IEnumerable<long>> Classify(ICollection<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken);
    }
}
