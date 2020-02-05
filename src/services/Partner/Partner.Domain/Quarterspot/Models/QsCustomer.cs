using System;

namespace Partner.Domain.Laso.Quarterspot.Models
{
    public class QsCustomer : QuarterspotEntity<string>
    {
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
