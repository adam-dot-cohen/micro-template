using System;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class DataPipelineStatus
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public Guid OrchestrationId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
    }
}
