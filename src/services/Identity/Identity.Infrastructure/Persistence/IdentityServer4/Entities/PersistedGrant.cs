using System;
using Laso.Identity.Domain.Entities;

namespace Laso.Identity.Infrastructure.Persistence.IdentityServer4.Entities
{
    public class PersistedGrant : TableStorageEntity
    {
        //TODO: this could be SubjectId, Key or some other combination depending on how ID server uses these (of course if it changes, the TableStoragePersistedGrantStore calls need to change)
        public override string PartitionKey => Key;
        public override string RowKey => "";

        public string Key { get; set; }
        public string Type { get; set; }
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? Expiration { get; set; }
        public string Data { get; set; }
    }
}