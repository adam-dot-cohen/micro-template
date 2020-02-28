using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsLoanMetadata
    {
        public Guid LeadId { get; set; }
        public long? BusinessId { get; set; }
        public Guid? GroupId { get; set; }
        public DateTime LeadCreatedDate { get; set; }
        public string Product { get; set; }
        public int ReportingGroupValue { get; set; }
        public decimal? RequestedAmount { get; set; }
        public int? MaxOfferedTerm { get; set; }
        public decimal? MaxOfferedAmount { get; set; }
        public string AcceptedTerm { get; set; }
        public decimal? AcceptedInstallment { get; set; }
        public string AcceptedInstallmentFrequency { get; set; }
        public decimal? AcceptedAmount { get; set; }
        public decimal? AcceptedInterestRate { get; set; }
        public string DeclineReason { get; set; }
    }
}
