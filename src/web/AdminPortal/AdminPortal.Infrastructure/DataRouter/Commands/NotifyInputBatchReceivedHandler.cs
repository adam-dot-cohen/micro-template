using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.AdminPortal.Core.DataRouter.Persistence;
using Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents;
using Laso.IntegrationEvents;
using Laso.Mediation;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.DataRouter.Commands
{
    public class NotifyInputBatchReceivedHandler : CommandHandler<NotifyInputBatchReceivedCommand>
    {
        private readonly IDataQualityPipelineRepository _repository;
        private readonly ILogger<NotifyInputBatchReceivedHandler> _logger;
        private readonly IEventPublisher _eventPublisher;

        public NotifyInputBatchReceivedHandler(
            IDataQualityPipelineRepository repository,
            ILogger<NotifyInputBatchReceivedHandler> logger,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public override async Task<CommandResponse> Handle(NotifyInputBatchReceivedCommand request, CancellationToken cancellationToken)
        {
            var fileBatch = await _repository.GetFileBatch(request.FileBatchId);

            var @event = new InputBatchReceivedEventV1
            {
                FileBatchId = request.FileBatchId,
                Timestamp = DateTimeOffset.UtcNow,
                PartnerId = fileBatch.PartnerId,
                PartnerName = fileBatch.PartnerName,
                Files = fileBatch.Files
                    .Select(x => new BlobFileInfo
                    {
                        Id = x.Id,
                        Uri = x.Uri,
                        ETag = x.ETag,
                        ContentType = x.ContentType,
                        ContentLength = x.ContentLength,
                        DataCategory = x.DataCategory,
                        EffectiveDate = x.EffectiveDate,
                        TransmissionTime = x.TransmissionTime
                    })
                    .ToList()
            };

            _logger.LogInformation("Publishing partner file batch for processing {@FileBatch}", @event);

            await _eventPublisher.Publish(@event, "CustomerData");

            return Succeeded();
        }
    }
}