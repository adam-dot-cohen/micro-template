using System;
using System.Collections.Generic;
using Laso.TableStorage.Domain;

namespace Laso.Identity.Infrastructure.Persistence
{
    public class IdentityResource : TableStorageEntity
    {
        public override string PartitionKey => Name;
        public override string RowKey => "";

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        [Delimited]
        public ICollection<string> UserClaims { get; set; }
        [Delimited]
        public IDictionary<string, string> Properties { get; set; }

        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
    }

    public class ApiResource : TableStorageEntity
    {
        public override string PartitionKey => Name;
        public override string RowKey => "";

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        [Delimited]
        public ICollection<string> UserClaims { get; set; }
        [Delimited]
        public IDictionary<string, string> Properties { get; set; }
    }

    public class ApiSecret : TableStorageEntity
    {
        public override string PartitionKey => ApiResourceName;
        public override string RowKey => Id;

        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public string ApiResourceName { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime? Expiration { get; set; }
        public string Type { get; set; }
    }

    public class ApiScope : TableStorageEntity
    {
        public override string PartitionKey => Name;
        public override string RowKey => ApiResourceName;

        public string ApiResourceName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
        [Delimited]
        public ICollection<string> UserClaims { get; set; }
    }
}