using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema
{
    public class LoanAccount_v0_3
    {
        public string? Loan_Account_Id { get; set; }
        public string? Business_Id { get; set; }
        public string? Customer_Id { get; set; }
        public string? Product_Type { get; set; }
        public DateTimeOffset? Effective_Date { get; set; }
        public DateTimeOffset? Issue_Date { get; set; }
        public DateTimeOffset? Maturity_Date { get; set; }
        public string? Interest_Rate_Method { get; set; }
        public string? Interest_Rate { get; set; }
        public string? Amortization_Method { get; set; }
        public string? Term { get; set; }
        public string? Installment { get; set; }
        public string? Installment_Frequency { get; set; }
        public string? Refinance_Loan_Id { get; set; }
        public decimal? Original_Principal_Balance { get; set; }
        public decimal? Current_Principal_Balance { get; set; }
        public decimal? Current_Interest_Balance { get; set; }
        public decimal? Original_Credit_Limit { get; set; }
        public decimal? Current_Credit_Limit { get; set; }
        public decimal? Payment_Amount { get; set; }
        public decimal? Payment_Interest { get; set; }
        public decimal? Payment_Principal { get; set; }
        public DateTimeOffset? Write_Off_Date { get; set; }
        public decimal? Write_Off_Principal_Balance { get; set; }
        public decimal? Write_Off_Interest_Balance { get; set; }
        public decimal? Recovery_Principal_Balance { get; set; }
        public DateTimeOffset? Recovery_Principal_Date { get; set; }
        public DateTimeOffset? Close_Date { get; set; }
        public string? Close_Reason { get; set; }
        public int? Past_Due_01_to_29_Days_Count { get; set; }
        public int? Past_Due_30_to_59_Days_Count { get; set; }
        public int? Past_Due_60_to_89_Days_Count { get; set; }
        public int? Past_Due_Over_90_Days_Count { get; set; }
        public int? Total_Days_Past_Due { get; set; }
    }
}