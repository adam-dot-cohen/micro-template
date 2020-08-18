using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Queries;

namespace Laso.AdminPortal.Infrastructure.DataRouter.Events
{
    public interface IDataAnalysisNotifier
    {
        Task UpdateAnalysisStatus(AnalysisStatusViewModel status, CancellationToken cancellationToken);
    }
}