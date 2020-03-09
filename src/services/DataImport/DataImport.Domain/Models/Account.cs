using System;

namespace Laso.DataImport.Domain.Models
{
    public class Account : ILasoEntity
    {
        public string AccountId { get; set; }
        public string BusinessId { get; set; }
        public string CustomerId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string AccountType { get; set; }
        public string InterestRateMethod { get; set; }
        public string InterestRate { get; set; }
        public DateTime? AccountOpenDate { get; set; }
        public string CurrentBalance { get; set; }
        public DateTime? CurrentBalanceDate { get; set; }
        public string AverageDailyBalance { get; set; }
        public DateTime? AccountClosedDate { get; set; }
        public string AccountClosedReason { get; set; }
    }
}
