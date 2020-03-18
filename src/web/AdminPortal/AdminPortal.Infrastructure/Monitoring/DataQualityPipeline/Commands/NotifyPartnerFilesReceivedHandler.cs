using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class NotifyPartnerFilesReceivedHandler : ICommandHandler<NotifyPartnerFilesReceivedCommand>
    {
        private readonly ILogger<NotifyPartnerFilesReceivedHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;

        public NotifyPartnerFilesReceivedHandler(
            ILogger<NotifyPartnerFilesReceivedHandler> logger,
            IMediator mediator,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _mediator = mediator;
            _eventPublisher = eventPublisher;
        }

        public async Task<CommandResponse> Handle(NotifyPartnerFilesReceivedCommand request, CancellationToken cancellationToken)
        {
            var fileInfo = await GetFileInfo(request.Event.Data.Url, cancellationToken);
            var partner = await GetPartner(fileInfo.PartnerId, cancellationToken);
            var @event = new PartnerFilesReceivedEvent
            {
                FileBatchId = request.FileBatchId,
                PartnerId = partner.Id,
                PartnerName = partner.Name,
                Files = new List<BlobFile>
                {
                    new BlobFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        Uri = request.Event.Data.Url,
                        ETag = request.Event.Data.ETag,
                        ContentType = request.Event.Data.ContentType,
                        ContentLength = request.Event.Data.ContentLength,
                        DataCategory = fileInfo.DataCategory
                    }
                }
            };

            _logger.LogInformation("Publishing partner file batch for processing {@FileBatch}", @event);

            await _eventPublisher.Publish(@event);

            return new CommandResponse();
        }

        private async Task<FileInfo> GetFileInfo(string fileUrl, CancellationToken cancellationToken)
        {
            var fileBatchInfo = await _mediator.Query(
                new GetFileBatchInfoQuery
                {
                    FilePaths = new[] {fileUrl}
                }, cancellationToken);

            var fileInfo = fileBatchInfo.Result.Files.First(); // for now, just one file
            return fileInfo;
        }

        private async Task<PartnerViewModel> GetPartner(string partnerId, CancellationToken cancellationToken)
        {
            var partnerResponse = await _mediator.Query(new GetPartnerViewModelQuery { PartnerId = partnerId }, cancellationToken);

            return partnerResponse.Result;
        }
    }
}