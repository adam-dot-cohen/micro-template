using System;

namespace Laso.DataImport.Domain.Quarterspot.Models
{
    public class QsCustomer
    {
        public string SsnEncrypted { get; set; }
        public decimal CreditScore { get; set; }
        public DateTime CreditScoreEffectiveTime { get; set; }
    }
}
