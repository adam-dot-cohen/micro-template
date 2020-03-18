using System;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class FileUploadedToEscrowEvent : CloudEventEnvelope<BlobEvent>
    {
    }

    public abstract class CloudEventEnvelope<TData>
    {
        public string Id { get; set; }
        public Uri Source { get; set; }
        public string SpecVersion { get; set; }
        public string Type { get; set; }
        public string DataSchema { get; set; }
        public string Subject { get; set; }
        public DateTime Time { get; set; }
        public TData Data { get; set; }
    }

    public class BlobEvent
    {
        public string Api { get; set; }
        public string ClientRequestId { get; set; }
        public string RequestId { get; set; }
        public string ETag { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public string BlobType { get; set; }
        public string Url { get; set; }
        public string Sequencer { get; set; }
        public StorageDiagnostics StorageDiagnostics { get; set; }
    }

    public class StorageDiagnostics
    {
        public string BatchId { get; set; }
    }
}
