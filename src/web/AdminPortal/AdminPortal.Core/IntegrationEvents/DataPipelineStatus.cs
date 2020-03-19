using System;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class DataPipelineStatus
    {
        public string EventType { get; set; }
        public string Stage { get; set; }
        public DateTime Timestamp { get; set; }
        public string OrchestrationId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
    }
}
