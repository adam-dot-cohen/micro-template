using System;
using System.Collections.Generic;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class PartnerFilesReceivedEvent : IEvent
    {
        public string FileBatchId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        // Will duplicate the FileBatchId value as data service is expecting this instead of FileBatchId as of 3/24/20
        public string CorrelationId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFileInfo> Files { get; set; }
    }

    public class BlobFileInfo
    {
        public string Id { get; set; }
        public string Uri { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ETag { get; set; }
        public string DataCategory { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset TransmissionTime { get; set; }
    }
}