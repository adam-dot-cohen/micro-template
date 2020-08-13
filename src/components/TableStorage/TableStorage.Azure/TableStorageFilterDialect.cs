using System;
using Laso.Filters;
using Laso.TableStorage.Azure.Extensions;

namespace Laso.TableStorage.Azure
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
        public string GetBooleanParameter(bool value) => value.ToString().ToLower();
        public string GetStandaloneBoolean(bool value) => value.ToString().ToLower();
        public string GetStringParameter(string value) => $"'{value}'";
        public string GetDateTimeParameter(DateTime value) => $"datetime'{value:s}Z'";
        public string GetGuidParameter(Guid value) => $"guid'{value:D}'";
        public string GetPrimitiveParameter(object value) => value.ToString();
        public string GetEnumParameter(Enum value) => value.GetValue().ToString();
    }
}