using System;

namespace DataImport.Domain.Api.Quarterspot.Models
{
    public class QsCustomer : QuarterspotEntity<string>
    {
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
