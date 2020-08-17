using System;

namespace Insights.AccountTransactionClassifier.Function.Domain
{
    public abstract class CloudEventEnvelope<TData>
        where TData : class
    {
        public string Id { get; set; } = null!;
        public Uri Source { get; set; } = null!;
        public string SpecVersion { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string DataSchema { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public DateTime Time { get; set; }
        public TData Data { get; set; } = null!;
    }
}