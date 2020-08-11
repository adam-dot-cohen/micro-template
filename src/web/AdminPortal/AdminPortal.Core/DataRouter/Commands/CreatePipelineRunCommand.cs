using Laso.Mediation;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class CreatePipelineRunCommand : ICommand<string>
    {
        public string FileBatchId { get; set; }
    }
}