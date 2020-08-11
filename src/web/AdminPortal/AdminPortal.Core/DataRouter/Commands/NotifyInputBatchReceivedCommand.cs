using Laso.Mediation;

namespace Laso.AdminPortal.Core.DataRouter.Commands
{
    public class NotifyInputBatchReceivedCommand : ICommand
    {
        public string FileBatchId { get; set; }
    }
}