using System;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema
{
    public class Demographic_v0_2
    {
	    public string? Customer_Id { get; set; }
        public string? Branch_Id { get; set; }
        public DateTimeOffset? Effective_Date { get; set; }
	    public string? Credit_Score { get; set; }
	    public string? Credit_Score_Source { get; set; }
    }
}
