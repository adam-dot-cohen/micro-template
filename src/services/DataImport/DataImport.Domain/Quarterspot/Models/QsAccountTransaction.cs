using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsAccountTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime? AvailableDate { get; set; }
        public DateTime? PostedDate { get; set; }
        public int TransactionCategoryValue { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
        public string MccCode { get; set; }
        public decimal? BalanceAfterTransaction { get; set; }
    }
}
