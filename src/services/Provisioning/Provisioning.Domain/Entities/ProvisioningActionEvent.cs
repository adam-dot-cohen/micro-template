using System;
using Laso.TableStorage.Domain;

namespace Provisioning.Domain.Entities
{
    public class ProvisioningActionEvent : TableStorageEntity
    {
        public override string PartitionKey => PartnerId;
        public override string RowKey => $"{Type}-{Completed.Ticks}";


        public string PartnerId { get; set; }
        public ProvisioningActionType Type { get; set; }
        public DateTime Completed { get; set; }
        public bool Succeeded => string.IsNullOrWhiteSpace(ErrorMessage);
        public string ErrorMessage { get; set; }

    }
}