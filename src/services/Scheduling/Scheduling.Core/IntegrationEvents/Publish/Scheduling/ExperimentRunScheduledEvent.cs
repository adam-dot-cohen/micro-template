using System.Collections.Generic;
using Laso.IntegrationEvents;

namespace Laso.Scheduling.Core.IntegrationEvents.Publish.Scheduling
{
    public class ExperimentRunScheduledEventV1 : IIntegrationEvent
    {
        public ExperimentRunScheduledEventV1(string partnerId, string experimentScheduleId, string fileBatchId, IReadOnlyCollection<ExperimentFile> files)
        {
            PartnerId = partnerId;
            ExperimentScheduleId = experimentScheduleId;
            FileBatchId = fileBatchId;
            Files = files;
        }

        public string PartnerId { get; }

        public string ExperimentScheduleId { get; }

        public string FileBatchId { get; }

        public IReadOnlyCollection<ExperimentFile> Files { get; }
    }

    public class ExperimentFile
    {
        public ExperimentFile(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; }

        public string? DataCategory { get; set; }
    }
}

