using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsCustomer
    {
        public string Id { get; set; }
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
