using System;

namespace DataImport.Domain.Api.Models
{
    public class AccountTransaction : ILasoEntity
    {
        public string TransactionId { get; set; }
        public string AccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? PostDate { get; set; }
        public string TransactionCategory { get; set; }
        public string Amount { get; set; }
        public string MemoField { get; set; }
        public string MccCode { get; set; }
        public string BalanceAfterTransaction { get; set; }
    }
}
