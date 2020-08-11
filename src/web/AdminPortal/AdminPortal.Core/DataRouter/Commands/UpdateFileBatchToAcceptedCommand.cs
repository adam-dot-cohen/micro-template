using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class UpdateFileBatchToAcceptedCommand : ICommand
    {
        public DataPipelineStatus Event { get; set; }
    }
}