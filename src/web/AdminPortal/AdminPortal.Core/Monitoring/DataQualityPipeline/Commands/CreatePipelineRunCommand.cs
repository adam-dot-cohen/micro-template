using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class CreatePipelineRunCommand : ICommand<string>
    {
        public string PartnerId { get; set; }
        public string FileBatchId { get; set; }
    }
}