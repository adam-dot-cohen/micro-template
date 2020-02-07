using System;

namespace DataImport.Domain.Api.Quarterspot.Models
{
    public class QsCustomer
    {
        public string Id { get; set; }
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
