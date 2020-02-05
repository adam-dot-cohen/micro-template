using System;

namespace Partner.Domain.Laso.Models
{
    public class LoanApplication : ILasoEntity
    {
        public string LoanApplicationId { get; set; }
        public string BusinessId { get; set; }
        public string CustomerId { get; set; }
        public string LoanAccountId { get; set; }        
        public DateTime EffectiveDate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string ProductType { get; set; }
        public DateTime DecisionDate { get; set; }
        public string DecisionResult { get; set; }
        public string DeclineReason { get; set; }
        public string ApplicationStatus { get; set; }
        public string RequestedAmount { get; set; }
        public string ApprovedTerm { get; set; }
        public string ApprovedInstallment { get; set; }
        public string ApprovedInstallmentFrequency { get; set; }
        public string ApprovedAmount { get; set; }
        public string ApprovedInterestRate { get; set; }
        public string ApprovedInterestedRateMethod { get; set; }
        public string AcceptedTerm { get; set; }
        public string AcceptedInstallment { get; set; }
        public string AcceptedInstallmentFrequency { get; set; }
        public string AcceptedAmount { get; set; }
        public string AcceptedInterestRate { get; set; }
        public string AcceptedInterestRateMethod { get; set; }
    }
}