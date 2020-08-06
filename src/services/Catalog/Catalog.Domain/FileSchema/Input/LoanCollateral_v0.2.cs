using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema.Input
{
    public class LoanCollateral_v0_2
    {
        public string? Loan_Collateral_Id { get; set; }
        public string? Loan_Account_Id { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public decimal? Initial_Assessed_Value { get; set; }
        public DateTimeOffset? Initial_Assessed_Date { get; set; }
        public decimal? Assessed_Value { get; set; }
        public DateTimeOffset? Assessed_Date { get; set; }
    }
}