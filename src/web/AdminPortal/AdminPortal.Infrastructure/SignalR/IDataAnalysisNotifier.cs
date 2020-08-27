using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Queries;
using Microsoft.AspNetCore.SignalR;

namespace Laso.AdminPortal.Infrastructure.SignalR
{
    public interface IDataAnalysisNotifier
    {
        Task UpdateAnalysisStatus(AnalysisStatusViewModel status, CancellationToken cancellationToken);
    }

    public class DataAnalysisHub : Hub { }

    public class DataAnalysisNotifier : IDataAnalysisNotifier
    {
        private readonly IHubContext<DataAnalysisHub> _hubContext;

        public DataAnalysisNotifier(IHubContext<DataAnalysisHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task UpdateAnalysisStatus(AnalysisStatusViewModel status, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("Updated", status, cancellationToken);
        }
    }
}