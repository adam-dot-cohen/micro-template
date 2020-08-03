using System;
using Laso.DataPrivacy.Attributes;

// ReSharper disable InconsistentNaming

namespace Laso.Catalog.Domain.FileSchema
{
    public class Firmographic_v0_2
    {
        public string? Business_Id { get; set; }
        public string? Customer_Id { get; set; }
        public DateTimeOffset? Effective_Date { get; set; }
        public DateTimeOffset? Date_Started { get; set; }
        public string? Industry_NAICS { get; set; }
        public string? Industry_SIC { get; set; }
        public string? Business_Type { get; set; }
        public string? Legal_Business_Name { get; set; }
        [Sensitive] public string? Business_Phone { get; set; }
        public string? Postal_Code { get; set; }
        [Sensitive] public string? EIN { get; set; }
    }
}