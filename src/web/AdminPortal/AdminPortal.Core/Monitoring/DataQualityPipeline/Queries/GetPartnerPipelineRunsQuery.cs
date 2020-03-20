using System;
using System.Collections.Generic;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries
{
    public class GetPartnerPipelineRunsQuery : IQuery<PartnerPipelineRunsViewModel>
    {
        public string PartnerId { get; set; }
    }

    public class PartnerPipelineRunsViewModel
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<PipelineRunViewModel> PipelineRuns { get; set; }
    }

    public class PipelineRunViewModel
    {
        public string RunId { get; set; }
        public List<PipelineRunStatusViewModel> Statuses { get; set; }
    }

    public class PipelineRunStatusViewModel
    {
        public DateTimeOffset Timestamp { get; set; }
        public string FileDataCategory { get; set; }
        public string Status { get; set; }
    }
}