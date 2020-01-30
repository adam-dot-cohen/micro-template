using System;

namespace Laso.Domain.Models
{
    public class Demographic : Entity<string>
    {
        public string Id { get; set; }
        public Customer Customer { get; set; }
        public string BranchId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int CreditScore { get; set; }
        public string Source { get; set; }
    }
}
