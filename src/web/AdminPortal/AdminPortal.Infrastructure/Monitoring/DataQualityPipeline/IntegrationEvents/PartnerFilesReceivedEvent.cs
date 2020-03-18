using System.Collections.Generic;
using Laso.AdminPortal.Core.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.IntegrationEvents
{
    public class PartnerFilesReceivedEvent : IIntegrationEvent
    {
        public string FileBatchId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFile> Files { get; set; }
    }

    public class BlobFile
    {
        public string Id { get; set; }
        public string Uri { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ETag { get; set; }
        public string DataCategory { get; set; }
    }
}