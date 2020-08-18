using System;
using System.Collections.Generic;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class DataPipelineStatus : IEvent
    {
        public string EventType { get; set; }
        public string Stage { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string OrchestrationId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }

        public DataPipelineStatusBody Body { get; set; }

        public string FileBatchId => EventType == "DataAccepted" ? CorrelationId : null;
        public string PipelineRunId => EventType != "DataAccepted" ? CorrelationId : null;
    }

    public class DataPipelineStatusBody
    {
        public IDictionary<string, string> Manifests { get; set; }
        public DataPipelineStatusDocument Document { get; set; }
    }

    public class DataPipelineStatusDocument
    {
        public string DataCategory { get; set; }
        public string Etag { get; set; }
        public string Id { get; set; }
        public string Policy { get; set; }
        public string Uri { get; set; }
    }
}
