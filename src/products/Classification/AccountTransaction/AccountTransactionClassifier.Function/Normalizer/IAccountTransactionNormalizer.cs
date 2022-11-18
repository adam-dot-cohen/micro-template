using Laso.Catalog.Domain.FileSchema.Input;

namespace Insights.AccountTransactionClassifier.Function.Normalizer
{
    public interface IAccountTransactionNormalizer
    {
        string? NormalizeTransactionText(AccountTransaction_v0_3 transaction);
    }
}
