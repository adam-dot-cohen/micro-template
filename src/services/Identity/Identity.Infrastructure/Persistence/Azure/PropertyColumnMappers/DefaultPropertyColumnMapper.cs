using System;
using System.Collections.Generic;
using System.Reflection;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;

namespace Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers
{
    public class DefaultPropertyColumnMapper : IPropertyColumnMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return true;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            return new Dictionary<string, object> { { entityProperty.Name, value } };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            return columns.Get(entityProperty.Name);
        }

        public string MapToQuery(PropertyInfo entityProperty, object value)
        {
            var type = entityProperty.PropertyType.GetNonNullableType();

            if (type == typeof(bool))
                return value.ToString().ToLower();
            if (type == typeof(string))
                return $"'{value}'";
            if (type == typeof(DateTime))
                return $"datetime'{((DateTime) value):s}Z'";
            if (type == typeof(Guid))
                return $"guid'{((Guid) value):D}'";
            if (type.IsValueType)
                return value.ToString();

            throw new ArgumentOutOfRangeException(type.Name);
        }
    }
}