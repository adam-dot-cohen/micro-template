using System;
using Azure.Data.Tables;
using Laso.TableStorage.Domain;

namespace Provisioning.Domain.Entities
{
    public class ProvisioningActionEvent : TableStorageEntity, ITableEntity
    {
        public override string PartitionKey
        {
            get => PartnerId;
            set => PartnerId = value;
        }

        public override string RowKey => $"{Type}-{Completed.Ticks}";


        public string PartnerId { get; set; }
        public ProvisioningActionType Type { get; set; }
        public DateTime Completed { get; set; }
        public bool Succeeded => string.IsNullOrWhiteSpace(ErrorMessage);
        public string ErrorMessage { get; set; }

    }
}