using System.Collections.Generic;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline
{
    public class Manifest
    {
            public string correlationId { get; set; }
            public List<DocumentsItem> documents { get; set; }
            public List<EventsItem> events { get; set; }
            public string orchestrationId { get; set; }
            public string tenantId { get; set; }
            public string tenantName { get; set; }
            public string type { get; set; }
        }
        public class Metrics
        {
            public int adjustedBoundaryRows { get; set; }
            public int curatedRows { get; set; }
            public int quality { get; set; }
            public int rejectedCSVRows { get; set; }
            public int rejectedConstraintRows { get; set; }
            public int rejectedSchemaRows { get; set; }
            public int sourceRows { get; set; }
        }

        public class DocumentsItem
        {
            public string dataCategory { get; set; }
            public string eTag { get; set; }
            public string id { get; set; }
            public Metrics metrics { get; set; }
            public string policy { get; set; }
            public string uri { get; set; }
            public List<string> errors { get; set; }
    }

        public class EventsItem
        {
            public string name { get; set; }
            public string timestamp { get; set; }
            public string message { get; set; }
        }

 
    
}
