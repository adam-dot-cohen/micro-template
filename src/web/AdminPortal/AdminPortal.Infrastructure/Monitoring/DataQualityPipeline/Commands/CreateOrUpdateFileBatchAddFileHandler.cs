using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.Core.Partners.Queries;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class CreateOrUpdateFileBatchAddFileHandler : ICommandHandler<CreateOrUpdateFileBatchAddFileCommand, string>
    {
        private readonly IDataQualityPipelineRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateOrUpdateFileBatchAddFileHandler> _logger;

        public CreateOrUpdateFileBatchAddFileHandler(
            IDataQualityPipelineRepository repository,
            IMediator mediator,
            ILogger<CreateOrUpdateFileBatchAddFileHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CommandResponse<string>> Handle(CreateOrUpdateFileBatchAddFileCommand request, CancellationToken cancellationToken)
        {
            var fileInfoResponse = await _mediator.Query(new GetFileBatchInfoQuery { FilePaths = new[] { request.Uri } }, cancellationToken);
            if (!fileInfoResponse.Success)
            {
                return fileInfoResponse.ToResponse<CommandResponse<string>>();
            }

            var fileInfo = fileInfoResponse.Result.Files.First();
            var partner = await GetPartner(fileInfo.PartnerId, cancellationToken);

            //TODO: query for batch based on partner and file's date - for now it's a single file per batch
            var blobFile = new BlobFile
            {
                Uri = request.Uri,
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                ETag = request.ETag,
                DataCategory = fileInfo.DataCategory
            };
            var fileBatch = new FileBatch
            {
                PartnerId = partner.Id,
                PartnerName = partner.Name,
                Files = new List<BlobFile> { blobFile }
            };

            await _repository.AddFileBatch(fileBatch);
            await _repository.AddFileBatchEvent(new DataPipelineStatus
            {
                CorrelationId = fileBatch.Id,
                PartnerId = partner.Id,
                Timestamp = DateTimeOffset.UtcNow,
                EventType = "DataAccepted", // TODO: Right now this what makes this a FileBatchEvent
                Stage = "PartnerFilesReceived",
                PartnerName = partner.Name
                // Do not include body since dealing with multiple files. Should probably be different event type
            });

            _logger.LogInformation("Created partner file batch.");

            //TODO: this should happen once the OK file has been received and verified
            await _mediator.Command(new NotifyPartnerFilesReceivedCommand { FileBatchId = fileBatch.Id }, cancellationToken);

            return CommandResponse.Succeeded(fileBatch.Id);
        }

        private async Task<PartnerViewModel> GetPartner(string partnerId, CancellationToken cancellationToken)
        {
            var partnerResponse = await _mediator.Query(new GetPartnerViewModelQuery { PartnerId = partnerId }, cancellationToken);

            return partnerResponse.Result;
        }
    }
}