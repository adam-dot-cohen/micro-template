using Laso.Mediation;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class CreateOrUpdateFileBatchAddFileCommand : ICommand<string>
    {
        public string Uri { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ETag { get; set; }
    }
}