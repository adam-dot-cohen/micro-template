using System;

namespace Laso.DataImport.Domain.Models
{
    public class Demographic : ILasoEntity
    {        
        public string CustomerId { get; set; }
        public string BranchId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int CreditScore { get; set; }
        public string Source { get; set; }
    }
}
