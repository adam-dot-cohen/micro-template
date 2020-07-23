using System;
using System.Collections.Generic;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries
{
    public class GetFileBatchInfoQuery : IQuery<FileBatchInfo>
    {
        public ICollection<string> FilePaths { get; set; }
    }

    public class FileBatchInfo
    {
        public ICollection<FileInfo> Files { get; set; }
    }

    public class FileInfo
    {
        public string Path { get; set; }
        public string PartnerId { get; set; }
        public string Filename { get; set; }
        public string DataCategory { get; set; }
        public string Frequency { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset TransmissionTime { get; set; }
    }
}