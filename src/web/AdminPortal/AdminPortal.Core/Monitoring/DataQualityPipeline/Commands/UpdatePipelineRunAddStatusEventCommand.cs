using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class UpdatePipelineRunAddStatusEventCommand : ICommand
    {
        public DataPipelineStatus Event { get; set; }
    }
}