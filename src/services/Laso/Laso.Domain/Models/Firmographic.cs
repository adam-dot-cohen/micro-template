using System;

namespace Laso.Domain.Models
{
    public class Firmographic : IEntity
    {        
        public string BusinessId { get; set; }
        public string CustomerId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? DateStarted { get; set; }
        public string IndustryNaics { get; set; }
        public string IndustrySic { get; set; }
        public string BusinessType { get; set; }
        public string LegalBusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEin { get; set; }
        public string PostalCode { get; set; }
    }
}
