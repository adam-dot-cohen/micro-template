using Infrastructure.Mediation.Command;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class NotifyInputBatchAcceptedCommand : ICommand
    {
        public string FileBatchId { get; set; }
    }
}