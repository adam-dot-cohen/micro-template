﻿using System;

namespace Laso.Domain.Models
{
    public class Demographic : IEntity
    {        
        public string CustomerId { get; set; }
        public string BranchId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int CreditScore { get; set; }
        public string Source { get; set; }
    }
}
