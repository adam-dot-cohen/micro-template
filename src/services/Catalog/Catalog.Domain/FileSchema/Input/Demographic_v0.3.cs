using System;
using Laso.DataPrivacy.Attributes;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema.Input
{
    public class Demographic_v0_3
    {
	    public string? Customer_Id { get; set; }
        public string? Branch_Id { get; set; }
        public DateTimeOffset? Effective_Date { get; set; }
        [Sensitive] public string? Name { get; set; }
        [Sensitive] public string? Phone { get; set; }
        [Sensitive] public string? Email { get; set; }
        [Sensitive] public string? SSN { get; set; }
        [Sensitive] public string? DriversLicense { get; set; }
        [Sensitive] public DateTimeOffset? DateOfBirth { get; set; }
        public string? StreetAddress1 { get; set; }
        public string? StreetAddress2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postal_Code { get; set; }
        public string? Credit_Score { get; set; }
        public string? Credit_Score_Source { get; set; }
    }
}
