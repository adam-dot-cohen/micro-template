using System;
using Azure;
using Azure.Data.Tables;

namespace Laso.TableStorage.Domain
{
    public abstract class TableStorageEntity : ITableEntity
    {
        public virtual string PartitionKey { get; set; }
        public virtual string RowKey { get; set; }

        public ETag ETag { get;  set; }
        public DateTimeOffset? Timestamp { get;  set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TableStorageEntity)obj);
        }

        public override int GetHashCode()
        {
            var hash = 13;

            var partitionKey = PartitionKey;
            if (partitionKey != null) hash = hash * 7 + partitionKey.GetHashCode();

            var rowKey = RowKey;
            if (rowKey != null) hash = hash * 7 + rowKey.GetHashCode();

            return hash;
        }

        protected bool Equals(TableStorageEntity other)
        {
            var thisPartitionKey = PartitionKey;
            var otherPartitionKey = other.PartitionKey;
            if (!thisPartitionKey.Equals(otherPartitionKey))
                return false;

            var thisRowKey = RowKey;
            var otherRowKey = other.RowKey;
            if (!thisRowKey.Equals(otherRowKey))
                return false;

            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DelimitedAttribute : Attribute
    {
        public char CollectionDelimiter { get; set; } = '|';
        public char DictionaryDelimiter { get; set; } = ';';
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentAttribute : Attribute { }
}