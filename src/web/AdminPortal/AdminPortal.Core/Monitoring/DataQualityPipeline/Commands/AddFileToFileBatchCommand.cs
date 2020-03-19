using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class AddFileToFileBatchCommand : ICommand<string>
    {
        public string Uri { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ETag { get; set; }
    }
}