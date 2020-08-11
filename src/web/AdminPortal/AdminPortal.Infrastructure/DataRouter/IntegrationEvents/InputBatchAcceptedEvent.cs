using System;
using System.Collections.Generic;
using Laso.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class InputBatchAcceptedEventV1 : IIntegrationEvent
    {
        public string FileBatchId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFileInfoV1> Files { get; set; }
    }

    public class BlobFileInfoV1
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
