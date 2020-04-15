using System;
using System.Collections.Generic;

namespace Laso.Provisioning.Domain.Entities
{
    public class PartnerHistory : TableStorageEntity
    {
        public override string PartitionKey => PartnerId;
        public override string RowKey => string.Empty;

        public string PartnerId { get; set; }

        public IList<ProvisioningEvent> Events { get; set; }
    }

    public class ProvisioningEvent
    {
        public DateTime BeganOn { get; set; }
        public DateTime EndedOn { get; set; }

        public bool Manual { get; set; }

        public IEnumerable<ProvisionableResource> ProvisionedResources { get; set; }
    }

    public class ProvisionableResource
    {
        public ProvisionableResourceType Type { get; set; }
        public string Name { get; set; }

        //TODO: other possible properties include:
        // Provider (Azure, Amazon, Google, etc.)
        // Location
        // Tags
        // Basically anything that would aid in locating and managing these resources
    }

    //TODO: this should be converted into a collection of value objects maintained in the service (out of code)
    public enum ProvisionableResourceType
    {
        BlobContainer,
        BlobFolder,
        SFTPRoot,
        SFTPUser,
    }
}
