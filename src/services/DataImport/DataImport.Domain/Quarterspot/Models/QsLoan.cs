using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsLoan
    {
        public Guid Id { get; set; }
        public long BusinessId { get; set; }
        public string ProductType { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public int DaysPastDue { get; set; }
        public string InterestRateMethod { get; set; }
        public string InterestRate { get; set; }
        public string AmortizationMethod { get; set; }
        public int Term { get; set; }
        public decimal Installment { get; set; }
        public string InstallmentFrequency { get; set; }
    }
}
