using System;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Filters;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public class TableStorageFilterDialect : IFilterDialect
    {
        public string Equal => "eq";
        public string NotEqual => "ne";
        public string GreaterThan => "gt";
        public string LessThan => "lt";
        public string GreaterThanOrEqual => "ge";
        public string LessThanOrEqual => "le";
        public string And => "and";
        public string Or => "or";
        public string GetBoolean(bool value) => value.ToString().ToLower();
        public string GetString(string value) => $"'{value}'";
        public string GetDateTime(DateTime value) => $"datetime'{value:s}Z'";
        public string GetGuid(Guid value) => $"guid'{value:D}'";
        public string GetPrimitive(object value) => value.ToString();
        public string GetEnum(Enum value) => value.GetValue().ToString();
    }
}