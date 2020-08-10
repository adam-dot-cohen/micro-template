using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class UpdatePipelineRunAddStatusEventCommand : ICommand
    {
        public DataPipelineStatus Event { get; set; }
    }
}