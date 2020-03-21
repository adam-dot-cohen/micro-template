using System.Collections.Generic;
using Laso.AdminPortal.Core.Mediator;

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
        public string DataCategory { get; set; }
        public string PartnerId { get; set; }
    }
}