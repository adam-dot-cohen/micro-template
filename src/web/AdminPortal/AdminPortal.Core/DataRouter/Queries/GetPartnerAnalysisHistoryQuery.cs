using System;
using System.Collections.Generic;
using Infrastructure.Mediation.Query;

namespace Laso.AdminPortal.Core.DataRouter.Queries
{
    public class GetPartnerAnalysisHistoryQuery : IQuery<PartnerAnalysisHistoryViewModel>
    {
        public string PartnerId { get; set; }
    }

    public class PartnerAnalysisHistoryViewModel
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public ICollection<FileBatchViewModel> FileBatches { get; set; } = new List<FileBatchViewModel>();
    }

    public class FileBatchViewModel
    {
        public string FileBatchId { get; set; }
        public string Status { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public ICollection<FileBatchFileViewModel> Files { get; set; } = new List<FileBatchFileViewModel>();
        public ICollection<ProductAnalysisViewModel> ProductAnalysisRuns { get; set; } = new List<ProductAnalysisViewModel>();
    }

    public class FileBatchFileViewModel
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public long ContentLength { get; set; }
        public string DataCategory { get; set; }
    }

    public class ProductAnalysisViewModel
    {
        public string ProductName { get; set; }
        public string PipelineRunId { get; set; }
        public DateTimeOffset Requested { get; set; }
        public ICollection<AnalysisStatusViewModel> Statuses { get; set; } = new List<AnalysisStatusViewModel>();
    }

    public class AnalysisStatusViewModel
    {
        public string CorrelationId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Status { get; set; }
        public string DataCategory { get; set; }
    }
}