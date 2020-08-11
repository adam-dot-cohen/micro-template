using System;
using System.Collections.Generic;
using Laso.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class InputBatchReceivedEventV1 : IIntegrationEvent
    {
        public string FileBatchId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFileInfo> Files { get; set; }
    }
}
