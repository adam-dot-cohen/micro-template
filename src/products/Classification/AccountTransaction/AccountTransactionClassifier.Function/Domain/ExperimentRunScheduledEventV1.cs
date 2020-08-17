using System.Collections.Generic;

namespace Insights.AccountTransactionClassifier.Function.Domain
{
    public class ExperimentRunScheduledEventV1
    {
        public string PartnerId { get; set; } = null!;
        public string ExperimentScheduleId { get; set; } = null!;
        public string FileBatchId { get;  set; } = null!;

        public IReadOnlyCollection<ExperimentFile> Files { get; set; } = null!;
    }
}