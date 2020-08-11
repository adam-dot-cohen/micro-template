using System;
using System.Collections.Generic;

namespace Laso.Scheduling.Api.IntegrationEvents.CustomerData
{
    public class InputBatchAcceptedEventV1
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
}
