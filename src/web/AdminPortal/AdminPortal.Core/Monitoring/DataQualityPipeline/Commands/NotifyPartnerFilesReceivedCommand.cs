using Laso.Mediation;

namespace Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands
{
    public class NotifyPartnerFilesReceivedCommand : ICommand
    {
        public string FileBatchId { get; set; }
    }
}