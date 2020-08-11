using Laso.Mediation;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class NotifyPartnerFilesReceivedCommand : ICommand
    {
        public string FileBatchId { get; set; }
    }
}