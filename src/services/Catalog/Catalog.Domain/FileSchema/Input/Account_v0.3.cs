using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema.Input
{
    public class Account_v0_3
    {
        public string? Account_Id { get; set; }
        public string? Business_Id { get; set; }
        public string? Customer_Id { get; set; }
        public DateTimeOffset? Effective_Date { get; set; }
        public string? Account_Type { get; set; }
        public string? Interest_Rate_Method { get; set; }
        public decimal? Interest_Rate { get; set; }
        public DateTimeOffset? Account_Open_Date { get; set; }
        public decimal? Current_Balance { get; set; }
        public DateTimeOffset? Current_Balance_Date { get; set; }
        public decimal? Average_Daily_Balance { get; set; }
        public DateTimeOffset? Account_Closed_Date { get; set; }
        public string? Account_Closed_Reason { get; set; }
    }
}