using System;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Filters;

namespace Laso.Identity.Infrastructure.IntegrationEvents
{
    internal class AzureServiceBusSqlFilterDialect : IFilterDialect
    {
        public string Equal => "=";
        public string NotEqual => "!=";
        public string GreaterThan => ">";
        public string LessThan => "<";
        public string GreaterThanOrEqual => ">=";
        public string LessThanOrEqual => "<=";
        public string And => "AND";
        public string Or => "OR";
        public string GetBoolean(bool value) => value ? "1" : "0";
        public string GetString(string value) => $"'{value}'";
        public string GetDateTime(DateTime value) => $"'{value:u}'";
        public string GetGuid(Guid value) => $"'{value:D}'";
        public string GetPrimitive(object value) => value.ToString();
        public string GetEnum(Enum value) => value.GetValue().ToString();
    }
}