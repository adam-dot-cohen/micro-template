using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class UpdatePipelineRunAddStatusEventCommand : ICommand
    {
        public DataPipelineStatus Event { get; set; }
    }
}