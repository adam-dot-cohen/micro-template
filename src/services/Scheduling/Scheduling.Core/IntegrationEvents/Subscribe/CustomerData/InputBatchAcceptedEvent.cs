using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Laso.Scheduling.Core.Experiments.Commands;
using MediatR;

namespace Laso.Scheduling.Core.IntegrationEvents.Subscribe.CustomerData
{
    public class InputBatchAcceptedEventV1 : IEvent
    {
        public InputBatchAcceptedEventV1(string fileBatchId, DateTimeOffset timestamp, string partnerId, string partnerName, IReadOnlyCollection<BlobFileInfoV1> files)
        {
            FileBatchId = fileBatchId;
            Timestamp = timestamp;
            PartnerId = partnerId;
            PartnerName = partnerName;
            Files = files;
        }

        public string FileBatchId { get; }
        public DateTimeOffset Timestamp { get; }
        public string PartnerId { get; }
        public string PartnerName { get; }
        public IReadOnlyCollection<BlobFileInfoV1> Files { get; }
    }

    public class BlobFileInfoV1
    {
        public BlobFileInfoV1(string id, string uri, string contentType, long contentLength, string eTag, string dataCategory, DateTimeOffset effectiveDate, DateTimeOffset transmissionTime)
        {
            Id = id;
            Uri = uri;
            ContentType = contentType;
            ContentLength = contentLength;
            ETag = eTag;
            DataCategory = dataCategory;
            EffectiveDate = effectiveDate;
            TransmissionTime = transmissionTime;
        }

        public string Id { get; }
        public string Uri { get; }
        public string ContentType { get; }
        public long ContentLength { get; }
        public string ETag { get; }
        public string DataCategory { get; }
        public DateTimeOffset EffectiveDate { get; }
        public DateTimeOffset TransmissionTime { get; }
    }

    public class SchedulePartnerExperimentOnInputBatchAcceptedHandler : IEventHandler<InputBatchAcceptedEventV1>
    {
        private readonly IMediator _mediator;

        public SchedulePartnerExperimentOnInputBatchAcceptedHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<EventResponse> Handle(InputBatchAcceptedEventV1 notification, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new SchedulePartnerExperimentCommand(notification.PartnerId, notification), cancellationToken);

            return EventResponse.From(response);
        }
    }
}
