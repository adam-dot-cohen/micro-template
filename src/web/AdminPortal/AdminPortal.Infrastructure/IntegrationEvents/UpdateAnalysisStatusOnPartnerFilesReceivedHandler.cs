using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Laso.AdminPortal.Core.DataRouter.Queries;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.SignalR;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class UpdateAnalysisStatusOnPartnerFilesReceivedHandler : IEventHandler<PartnerFilesReceivedEvent>
    {
        private readonly IDataAnalysisNotifier _dataAnalysisNotifier;

        public UpdateAnalysisStatusOnPartnerFilesReceivedHandler(IDataAnalysisNotifier dataAnalysisNotifier)
        {
            _dataAnalysisNotifier = dataAnalysisNotifier;
        }

        public async Task<EventResponse> Handle(PartnerFilesReceivedEvent notification, CancellationToken cancellationToken)
        {
            var status = new AnalysisStatusViewModel
            {
                CorrelationId = notification.FileBatchId,
                Timestamp = notification.Timestamp,
                DataCategory = "N/A",
                Status = "PartnerFilesReceived"
            };

            await _dataAnalysisNotifier.UpdateAnalysisStatus(status, cancellationToken);

            return EventResponse.Succeeded();
        }
    }
}