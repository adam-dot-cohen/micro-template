using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline;
using Laso.AdminPortal.Core.Partners.Queries;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline
{
    public class NotifyPartnerFilesReceivedHandler : ICommandHandler<NotifyPartnerFilesReceivedCommand>
    {
        private readonly ILogger<NotifyPartnerFilesReceivedHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;

        private const string DataCategoryName = "category";

        // Data category immediately precedes date. Allow digits in case we add numeric versioning later
        private static readonly string DataCategoryRegex = $@"^.+_(?<{DataCategoryName}>[a-zA-Z][a-zA-Z0-9]{{2,}})_\d{{8}}_\d{{9,}}\.[\.a-zA-Z]+$";

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
            var partner = await GetPartner(request.Event, cancellationToken);
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
                        DataCategory = GetDataCategory(request.Event.Data.Url)
                    }
                }
            };

            _logger.LogInformation("Publishing partner file batch for processing {@FileBatch}", @event);

            await _eventPublisher.Publish(@event);

            return new CommandResponse();
        }

        private async Task<PartnerViewModel> GetPartner(FileUploadedToEscrowEvent requestEvent, CancellationToken cancellationToken)
        {
            var fileUri = new Uri(requestEvent.Data.Url);
            var normalizedSegments = fileUri.Segments
                .Select(Normalize)
                .ToArray();
            var partnersResponse = await _mediator.Query(new GetAllPartnerViewModelsQuery(), cancellationToken);

            var normalizedPartners = partnersResponse.Result
                .ToDictionary(
                    p => Normalize(p.Id),
                    p => p);

            var match = normalizedPartners.SingleOrDefault(kvp => normalizedSegments.Contains(kvp.Key));

            return match.Value;
        }

        private string Normalize(string value)
        {
            return new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        }

        private string GetDataCategory(string fileUrl)
        {
            var fileUri = new Uri(fileUrl);
            var fileName = fileUri.Segments.LastOrDefault();

            if (string.IsNullOrEmpty(fileName) || fileName.EndsWith("/"))
            {
                throw new InvalidOperationException($"Filename is not valid: {fileUrl}");
            }

            var match = Regex.Match(fileName, DataCategoryRegex);

            if (!match.Success)
            {
                throw new Exception($"Data category could not be extracted from file: {fileName}");
            }

            return match.Groups[DataCategoryName].Value;
        }
    }

    public class PartnerFilesReceivedEvent : IIntegrationEvent
    {
        public string FileBatchId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public List<BlobFile> Files { get; set; }
    }

    public class BlobFile
    {
        public string Id { get; set; }
        public string Uri { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string ETag { get; set; }
        public string DataCategory { get; set; }
    }
}