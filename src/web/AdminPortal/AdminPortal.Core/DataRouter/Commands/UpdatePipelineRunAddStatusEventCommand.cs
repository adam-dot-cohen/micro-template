using Laso.AdminPortal.Core.IntegrationEvents;
using Infrastructure.Mediation.Command;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class UpdatePipelineRunAddStatusEventCommand : ICommand
    {
        public DataPipelineStatus Event { get; set; }
    }
}