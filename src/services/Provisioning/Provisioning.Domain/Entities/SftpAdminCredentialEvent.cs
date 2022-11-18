using System;
using Azure.Data.Tables;
using Laso.TableStorage.Domain;

namespace Provisioning.Domain.Entities
{
    public class SftpAdminCredentialEvent : TableStorageEntity, ITableEntity
    {public override string PartitionKey
        {
            get => VMInstance;
            set => VMInstance = value;
        }

        //public override string PartitionKey => VMInstance;
        public override string RowKey => On.ToLongDateString();

        public string VMInstance { get; set; }
        public DateTime On { get; set; }
        public string Secret { get; set; }
        public string Version { get; set; }
        public bool Failed { get; set; }
    }
}