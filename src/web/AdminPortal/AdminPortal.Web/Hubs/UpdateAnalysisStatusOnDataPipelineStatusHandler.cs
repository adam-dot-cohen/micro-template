using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Queries;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.DataRouter.Events;
using Laso.Mediation;

namespace Laso.AdminPortal.Web.Hubs
{
    public class UpdateAnalysisStatusOnDataPipelineStatusHandler : IEventHandler<DataPipelineStatus>
    {
        private readonly IDataAnalysisNotifier _dataAnalysisNotifier;

        public UpdateAnalysisStatusOnDataPipelineStatusHandler(IDataAnalysisNotifier dataAnalysisNotifier)
        {
            _dataAnalysisNotifier = dataAnalysisNotifier;
        }

        public async Task<EventResponse> Handle(DataPipelineStatus notification, CancellationToken cancellationToken)
        {
            var status = new AnalysisStatusViewModel
            {
                CorrelationId = notification.CorrelationId,
                Timestamp = notification.Timestamp,
                DataCategory = notification.Body?.Document?.DataCategory,
                Status = notification.Stage ?? notification.EventType
            };

            await _dataAnalysisNotifier.UpdateAnalysisStatus(status, cancellationToken);

            return EventResponse.Succeeded();
        }
    }
}