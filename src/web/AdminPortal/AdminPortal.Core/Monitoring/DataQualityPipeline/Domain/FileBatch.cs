using System;
using System.Collections.Generic;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain
{
    public class FileBatch
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // TODO: Make private setter
        public DateTimeOffset Created { get; private set; } = DateTimeOffset.UtcNow;
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFile> Files { get; set; }
    }

    public class BlobFile
    {
            public string Id { get; private set; } = Guid.NewGuid().ToString();
            public string Uri { get; set; }
            public string ContentType { get; set; }
            public long ContentLength { get; set; }
            public string ETag { get; set; }
            public string DataCategory { get; set; }
    }

    public class PipelineRun
    {
        public string Id { get; set; } //TODO: change back to private set; } = Guid.NewGuid().ToString();
        public DateTimeOffset Created { get; private set; } = DateTimeOffset.UtcNow;
        public string PartnerId { get; set; }
        public string FileBatchId { get; set; }
        public List<BlobFile> Files { get; set; }
    }
}
