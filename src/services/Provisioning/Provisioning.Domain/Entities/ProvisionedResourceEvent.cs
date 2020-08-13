using System;
using Laso.TableStorage.Domain;

namespace Provisioning.Domain.Entities
{
    public class ProvisionedResourceEvent : TableStorageEntity
    {
        public override string PartitionKey => PartnerId;
        public override string RowKey => $"{ParentLocation}-{Location}"; 

        public string PartnerId { get; set; }

        public ProvisionedResourceType Type { get; set; }
        public DateTime ProvisionedOn { get; set; }

        public bool Manual { get; set; }

        public string DisplayName { get; set; }

        public string ParentLocation { get; set; }
        public string Location { get; set; }
        public bool Sensitive { get; set; }
    }
}