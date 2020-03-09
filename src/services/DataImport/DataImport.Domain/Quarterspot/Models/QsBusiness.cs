using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsBusiness
    {
        public long Id { get; set; }
        public DateTime? Established { get; set; }
        public int? IndustryNaicsCode { get; set; }
        public int? IndustrySicCode { get; set; }
        public int? BusinessEntityType { get; set; }
        public string LegalName { get; set; }
        public string Phone { get; set; }
        public string TaxId { get; set; }
        public string Zip { get; set; }
}
}
