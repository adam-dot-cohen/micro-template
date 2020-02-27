using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsAccount
    {
        public Guid AccountId { get; set; }
        public long BusinessId { get; set; }
        public Guid PrincipalId { get; set; }
        public int BankAccountCategory { get; set; }
        public string InterestRateMethod { get; set; }
        public string InterestRate { get; set; }
        public DateTime? OpenDate { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime? CurrentBalanceDate { get; set; }
        public string AverageDailyBalance { get; set; }
        public DateTime? AccountClosedDate { get; set; }
        public string AccountClosedReason { get; set; }
    }
}
