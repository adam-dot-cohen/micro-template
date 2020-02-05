using System;

namespace Partner.Domain.Laso.Quarterspot.Models
{
    public class QsBusiness : QuarterspotEntity<long>
    {
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
