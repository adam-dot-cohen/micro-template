using System;
using System.Collections.Generic;

namespace Laso.Domain.Models
{
    public class LoanAccount : Entity<string>
    {
        public string Id { get; set; }
        public Business Business { get; set; }
        public Customer Customer { get; set; }
        public LoanApplication Application { get; set; }
        public IEnumerable<LoanCollateral> Collateral { get; set; }
        public IEnumerable<LoanAttribute> Attributes { get; set; }
        public IEnumerable<LoanTransaction> Transactions { get; set; }
        public string ProductType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public string InterestRateMethod { get; set; }
        public string InterestRate { get; set; }
        public string AmortizationMethod { get; set; }
        public string Term { get; set; }
        public string Installment { get; set; }
        public string InstallmentFrequency { get; set; }
        public string RefinanceLoanId { get; set; }
        public string OriginalPrincipalBalance { get; set; }
        public string CurrentPrincipalBalance { get; set; }
        public string CurrentInterestBalance { get; set; }
        public string OriginalCreditLimit { get; set; }
        public string CurrentCreditLimit { get; set; }
        public string PaymentAmount { get; set; }
        public string PaymentInterest { get; set; }
        public string PaymentPrincipal { get; set; }
        public DateTime WriteOffDate { get; set; }
        public string WriteOffPrincipalBalance { get; set; }
        public string WriteOffInterestBalance { get; set; }
        public string RecoveryPrincipalBalance { get; set; }
        public DateTime RecoveryPrincipalDate { get; set; }
        public DateTime CloseDate { get; set; }
        public string CloseReason { get; set; }
        public string PastDue01To29DaysCount { get; set; }
        public string PastDue30To59DaysCount { get; set; }
        public string PastDue60To89DaysCount { get; set; }
        public string PastDueOver90DaysCount { get; set; }
        public string TotalDaysPastDue { get; set; }
    }
}
