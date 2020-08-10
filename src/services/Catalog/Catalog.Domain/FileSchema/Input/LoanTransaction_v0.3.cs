using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema.Input
{
    public class LoanTransaction_v0_3
    {
        public string? Loan_Transaction_Id { get; set; }
        public string? Loan_Account_Id { get; set; }
        public DateTimeOffset? Transaction_Date { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }
}