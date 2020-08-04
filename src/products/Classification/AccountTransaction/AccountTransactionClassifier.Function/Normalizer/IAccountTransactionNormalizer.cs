using Laso.Catalog.Domain.FileSchema;

namespace Insights.AccountTransactionClassifier.Function.Normalizer
{
    public interface IAccountTransactionNormalizer
    {
        string? NormalizeTransactionText(AccountTransaction_v0_3 transaction);
    }
}
