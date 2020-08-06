using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Laso.Catalog.Domain.FileSchema.Input;
using Laso.Catalog.Domain.FileSchema.Output;

// ReSharper disable InconsistentNaming

namespace Insights.AccountTransactionClassifier.Function.Classifier
{
    public interface IAccountTransactionClassifier
    {
        Task<IEnumerable<AccountTransactionClass_v0_1>> Classify(IEnumerable<AccountTransaction_v0_3> transactions, CancellationToken cancellationToken);
    }
}
