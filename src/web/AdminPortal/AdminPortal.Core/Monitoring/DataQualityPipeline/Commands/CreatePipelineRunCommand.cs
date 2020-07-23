using Laso.Mediation;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class CreatePipelineRunCommand : ICommand<string>
    {
        public string FileBatchId { get; set; }
    }
}