using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class NotifyPartnerFilesReceivedCommand : ICommand
    {
        public string FileBatchId { get; set; }

        // Temporary - query for this by runId later
        public FileUploadedToEscrowEvent Event { get; set; }
    }
}