using System;

namespace Partner.Domain.Quarterspot.Models
{
    public class QsCustomer : QuarterspotEntity<string>
    {
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
