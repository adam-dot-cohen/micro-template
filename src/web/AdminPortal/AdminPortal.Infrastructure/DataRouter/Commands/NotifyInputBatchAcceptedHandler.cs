using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Command;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.AdminPortal.Core.DataRouter.Persistence;
using Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents;
using Laso.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.DataRouter.Commands
{
    public class NotifyInputBatchAcceptedHandler : CommandHandler<NotifyInputBatchAcceptedCommand>
    {
        private readonly IDataQualityPipelineRepository _repository;
        private readonly ILogger<NotifyInputBatchAcceptedHandler> _logger;
        private readonly IEventPublisher _eventPublisher;

        public NotifyInputBatchAcceptedHandler(
            IDataQualityPipelineRepository repository,
            ILogger<NotifyInputBatchAcceptedHandler> logger,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public override async Task<CommandResponse> Handle(NotifyInputBatchAcceptedCommand request, CancellationToken cancellationToken)
        {
            var fileBatch = await _repository.GetFileBatch(request.FileBatchId);

            var @event = new InputBatchAcceptedEventV1
            {
                FileBatchId = request.FileBatchId,
                Timestamp = DateTimeOffset.UtcNow,
                PartnerId = fileBatch.PartnerId,
                PartnerName = fileBatch.PartnerName,
                Files = fileBatch.Files
                    .Select(x => new BlobFileInfoV1
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