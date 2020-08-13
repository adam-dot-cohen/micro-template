using System.Text.RegularExpressions;
using Laso.Catalog.Domain.FileSchema.Input;

namespace Insights.AccountTransactionClassifier.Function.Normalizer
{
    public class AccountTransactionNormalizer : IAccountTransactionNormalizer
    {
        public string? NormalizeTransactionText(AccountTransaction_v0_3 transaction)
        {
            return NormalizedTransactionText(transaction.Memo_Field);
        }

        private static string? NormalizedTransactionText(string? memo)
        {
            if (string.IsNullOrEmpty(memo))
                return memo;

            var transactionText = memo.ToLower();

            transactionText = Regex.Replace(transactionText, @"\( <a href=""([a-zA-Z:/._\-\?\=\d])*"" title=""", string.Empty);    // Whacky case with HTML
            transactionText = Regex.Replace(transactionText, @"[-()_,$/:=?#@\\~\[\]]", " ");    // Replace symbols
            transactionText = Regex.Replace(transactionText, @"['.*&;""]", string.Empty);       // Remove symbols
            transactionText = Regex.Replace(transactionText, @"([a-z])\1\1+", string.Empty);    // Remove 3+ consecutive letters
            transactionText = Regex.Replace(transactionText, @"(^|\s)\d+", " ");                // Replace numbers
            transactionText = Regex.Replace(transactionText, @"\s+", " ");                      // Replace whitespace

            return transactionText.Trim();
        }
    }
}
