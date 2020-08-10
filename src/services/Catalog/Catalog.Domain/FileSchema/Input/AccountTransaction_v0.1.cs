using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema.Input
{
    public class AccountTransaction_v0_1
    {
        public string? Transaction_Id { get; set; }
        public string? Account_Id { get; set; }
        public DateTimeOffset? Transaction_Date { get; set; }
        public DateTimeOffset? Post_Date { get; set; }
        public string? Transaction_Category { get; set; }
        public decimal? Amount { get; set; }
        public string? Memo_Field { get; set; }
        public string? MCC_Code { get; set; }
        public decimal? Balance_After_Transaction { get; set; }
    }
}