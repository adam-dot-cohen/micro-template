using System;

namespace DataImport.Subscriptions.Domain
{
    public class ImportSubscription
    {
        public string Id { get; set; }
        public Partner ExportFrom { get; set; }
        public string Frequency { get; set; }
        public ImportType[] Imports { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }
        public DateTime NextScheduledImport { get; set; }
    }
}
