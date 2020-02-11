using System;

namespace DataImport.Domain.Api
{
    public class ImportHistory
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime Completed { get; set; }
        public bool Success { get; set; }
    }
}
