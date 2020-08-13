using System;
using System.Collections.Generic;
using Laso.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class PartnerFilesReceivedEvent : IIntegrationEvent
    {
        public string FileBatchId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        // Will duplicate the FileBatchId value as data service is expecting this instead of FileBatchId as of 3/24/20
        public string CorrelationId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFileInfoV1> Files { get; set; }
    }
}