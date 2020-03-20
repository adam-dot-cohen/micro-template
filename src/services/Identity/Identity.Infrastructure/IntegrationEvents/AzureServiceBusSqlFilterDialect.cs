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
        public string GetBooleanParameter(bool value) => value ? "1" : "0";
        public string GetStandaloneBoolean(bool value) => value ? "1=1" : "1=0";
        public string GetStringParameter(string value) => $"'{value}'";
        public string GetDateTimeParameter(DateTime value) => $"'{value:u}'";
        public string GetGuidParameter(Guid value) => $"'{value:D}'";
        public string GetPrimitiveParameter(object value) => value.ToString();
        public string GetEnumParameter(Enum value) => value.GetValue().ToString();
    }
}