using System;

namespace Laso.Identity.Domain.Entities
{
    public class Partner : TableStorageEntity
    {
        public override string PartitionKey => Id;
        public override string RowKey => string.Empty;

        public string Id { get; set; } = Guid.NewGuid().ToString("D");

        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string PublicKey { get; set; }
        public string ResourcePrefix { get; set; }
    }
}
