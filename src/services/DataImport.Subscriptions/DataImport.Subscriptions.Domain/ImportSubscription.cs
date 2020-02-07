using System;

namespace DataImport.Subscriptions.Domain
{

    public class ImportSubscription
    {
        public PartnerIdentifier ExportFrom { get; set; }
        public PartnerIdentifier ImportTo { get; set; }
        public ImportFrequency Frequency { get; set; }
        public ImportType[] Imports { get; set; }
        public DateTime LastSuccessfulImport { get; set; }
    }
}
