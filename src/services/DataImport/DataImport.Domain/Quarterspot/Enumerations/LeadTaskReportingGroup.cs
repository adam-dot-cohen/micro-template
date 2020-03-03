using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.DataImport.Domain.Quarterspot.Enumerations
{
    public static class ReportingGroupValue
    {
        public const int Created = 0;
        public const int PreScreen = 1;
        public const int Credit = 2;
        public const int Offer = 4;
        public const int Process = 5;
        public const int Qa = 6;
        public const int Fund = 7;
        public const int Funded = 8;
        public const int Rejected = 9;
        public const int Suspicious = 10;
        public const int NoCreditReport = 11;
        public const int DeclinedTerms = 12;
        public const int NoResponse = 13;
        public const int OnHold = 14;
    }

    public static class ReportingSubGroupValue
    {
        // PreScreen
        // DEPRECATED: public const int ApplySubmit = 14;
        public const int PreScreenAcquire = 15;
        public const int PreScreenReview = 17;

        // Underwrite
        public const int CreditAnalyze = 18;
        public const int CreditUnderwrite = 19;
        public const int CreditApprove = 20;

        // Offer
        public const int OfferSign = 22;

        // Close
        public const int ProcessAcquire = 24;
        public const int ProcessReview = 27;

        // QA
        public const int QaApprove = 29;

        // Fund
        public const int FundList = 32;
        public const int FundClose = 33;
        public const int FundFund = 34;


        // WhiteLabel

        // PreScreen
        public const int PrescreenState = 35;

        // Offer
        public const int OfferState = 36;

        // Process
        public const int ProcessState = 37;

        // QA
        public const int QaState = 38;

    }

    public class LeadTaskReportingGroup : Enumeration<LeadTaskReportingGroup>
    {
        public static readonly LeadTaskReportingGroup Created = new LeadTaskReportingGroup(ReportingGroupValue.Created, "Created", 0);

        public static readonly LeadTaskReportingGroup Prescreen = new LeadTaskReportingGroup(ReportingGroupValue.PreScreen, "PreScreen", 1, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup PrescreenAcquire = new LeadTaskReportingGroup(ReportingSubGroupValue.PreScreenAcquire, "Acquire", 2, Prescreen);
        public static readonly LeadTaskReportingGroup PrescreenReview = new LeadTaskReportingGroup(ReportingSubGroupValue.PreScreenReview, "Review", 5, Prescreen, previousReportingGroup: PrescreenAcquire);
        public static readonly LeadTaskReportingGroup PrescreenState = new LeadTaskReportingGroup(ReportingSubGroupValue.PrescreenState, "Prescreen", 6, Prescreen, previousReportingGroup: PrescreenReview);

        public static readonly LeadTaskReportingGroup Credit = new LeadTaskReportingGroup(ReportingGroupValue.Credit, "Credit", 2, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup CreditAnalyze = new LeadTaskReportingGroup(ReportingSubGroupValue.CreditAnalyze, "Analyze", 1, Credit, previousReportingGroup: PrescreenReview);
        public static readonly LeadTaskReportingGroup CreditUnderwrite = new LeadTaskReportingGroup(ReportingSubGroupValue.CreditUnderwrite, "Underwrite", 4, Credit, previousReportingGroup: CreditAnalyze);
        public static readonly LeadTaskReportingGroup CreditApprove = new LeadTaskReportingGroup(ReportingSubGroupValue.CreditApprove, "Approve", 6, Credit, previousReportingGroup: CreditUnderwrite);

        public static readonly LeadTaskReportingGroup Offer = new LeadTaskReportingGroup(ReportingGroupValue.Offer, "Offer", 3, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup OfferSign = new LeadTaskReportingGroup(ReportingSubGroupValue.OfferSign, "Sign", 1, Offer, previousReportingGroup: CreditApprove);
        public static readonly LeadTaskReportingGroup OfferState = new LeadTaskReportingGroup(ReportingSubGroupValue.OfferState, "Offer", 2, Offer, previousReportingGroup: OfferSign);

        public static readonly LeadTaskReportingGroup Process = new LeadTaskReportingGroup(ReportingGroupValue.Process, "Process", 4, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup ProcessAcquire = new LeadTaskReportingGroup(ReportingSubGroupValue.ProcessAcquire, "Acquire", 1, Process, previousReportingGroup: OfferSign);
        public static readonly LeadTaskReportingGroup ProcessReview = new LeadTaskReportingGroup(ReportingSubGroupValue.ProcessReview, "Review", 4, Process, previousReportingGroup: ProcessAcquire);
        public static readonly LeadTaskReportingGroup ProcessState = new LeadTaskReportingGroup(ReportingSubGroupValue.ProcessState, "Process", 5, Process, previousReportingGroup: ProcessReview);

        public static readonly LeadTaskReportingGroup Qa = new LeadTaskReportingGroup(ReportingGroupValue.Qa, "QA", 5, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup QaApprove = new LeadTaskReportingGroup(ReportingSubGroupValue.QaApprove, "Approve", 1, Qa, previousReportingGroup: ProcessReview);
        public static readonly LeadTaskReportingGroup QaState = new LeadTaskReportingGroup(ReportingSubGroupValue.QaState, "QA", 2, Qa, previousReportingGroup: QaApprove);

        public static readonly LeadTaskReportingGroup Fund = new LeadTaskReportingGroup(ReportingGroupValue.Fund, "Fund", 6, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup FundList = new LeadTaskReportingGroup(ReportingSubGroupValue.FundList, "List", 1, Fund, previousReportingGroup: QaApprove);
        public static readonly LeadTaskReportingGroup FundPurchase = new LeadTaskReportingGroup(ReportingSubGroupValue.FundClose, "Purchase", 2, Fund, previousReportingGroup: FundList);
        public static readonly LeadTaskReportingGroup FundFund = new LeadTaskReportingGroup(ReportingSubGroupValue.FundFund, "Fund", 3, Fund, previousReportingGroup: FundPurchase);

        public static readonly LeadTaskReportingGroup Funded = new LeadTaskReportingGroup(ReportingGroupValue.Funded, "Funded", 7, isTopLevelGroup: true, isInSalesPipeline: true);
        public static readonly LeadTaskReportingGroup Rejected = new LeadTaskReportingGroup(ReportingGroupValue.Rejected, "Rejected", 8, isTopLevelGroup: true);
        public static readonly LeadTaskReportingGroup Suspicious = new LeadTaskReportingGroup(ReportingGroupValue.Suspicious, "Suspicious", 9);
        public static readonly LeadTaskReportingGroup NoCreditReport = new LeadTaskReportingGroup(ReportingGroupValue.NoCreditReport, "No Credit Report", 10);
        public static readonly LeadTaskReportingGroup DeclinedTerms = new LeadTaskReportingGroup(ReportingGroupValue.DeclinedTerms, "Declined Terms", 11);
        public static readonly LeadTaskReportingGroup NoResponse = new LeadTaskReportingGroup(ReportingGroupValue.NoResponse, "No Response", 12);
        public static readonly LeadTaskReportingGroup OnHold = new LeadTaskReportingGroup(ReportingGroupValue.OnHold, "On Hold", 13);

        private LeadTaskReportingGroup(int value, string displayName, int order, LeadTaskReportingGroup parent = null, bool isTopLevelGroup = false, bool isInSalesPipeline = false, LeadTaskReportingGroup previousReportingGroup = null)
            : base(value, displayName)
        {
            Order = order;
            Parent = parent;
            IsTopLevelGroup = isTopLevelGroup;
            IsInSalesPipeline = isInSalesPipeline;
            Previous = previousReportingGroup;
            if (Previous != null) Previous.SubsequentGroup = this;
        }

        public LeadTaskReportingGroup Previous { get; set; }

        public static IEnumerable<LeadTaskReportingGroup> GetTopLevelReportingGroups()
        {
            return GetAll().Where(g => g.IsTopLevelGroup).OrderBy(g => g.Order).ToList();
        }

        public static IEnumerable<LeadTaskReportingGroup> GetSalesPipelineReportingGroups()
        {
            return GetAll().Where(g => g.IsInSalesPipeline).OrderBy(g => g.Order).ToList();
        }

        public IEnumerable<LeadTaskReportingGroup> GetChildGroups()
        {
            return GetAll().Where(g => (g.Parent != null) && (g.Parent == this)).ToList();
        }

        public int Order { get; }
        public LeadTaskReportingGroup Parent { get; }
        public bool IsTopLevelGroup { get; }
        public bool IsInSalesPipeline { get; }
        public LeadTaskReportingGroup SubsequentGroup { get; private set; }
    }
}
