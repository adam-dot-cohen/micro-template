using System;

namespace Laso.DataImport.Services.DTOs
{
    public class ImportHistory : IDto<string>
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime Completed { get; set; }
        public bool Success { get; set; }
        public string[] FailReasons { get; set; }
    }
}
