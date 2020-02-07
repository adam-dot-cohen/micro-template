﻿using System;

namespace DataImport.Domain.Api.Models
{
    public class Account : ILasoEntity
    {
        public string AccountId { get; set; }
        public string BusinessId { get; set; }
        public string CustomerId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string AccountType { get; set; }
        public decimal InterestRateMethod { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime? CurrentBalanceDate { get; set; }
        public string AverageDailyBalance { get; set; }
        public DateTime? AccountClosedDate { get; set; }
        public string AccountClosedReason { get; set; }
    }
}
