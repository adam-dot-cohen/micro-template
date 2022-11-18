using System;
using Azure.Data.Tables;
using Laso.TableStorage.Domain;

namespace Laso.Identity.Domain.Entities
{
    public class Partner : TableStorageEntity, ITableEntity
    {
        public override string PartitionKey
        {
            get => Id;
            set => Id = value;
        }
        

        public string Id { get; set; } = Guid.NewGuid().ToString("D");

        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string PublicKey { get; set; }
        public string NormalizedName { get; set; }
    }
}
