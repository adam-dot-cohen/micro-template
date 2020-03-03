using System;
using System.Collections.Generic;
using System.Reflection;
using Laso.DataImport.Core.Extensions;

namespace Laso.DataImport.Services.Persistence.Azure.PropertyColumnMappers
{
    public class EnumPropertyColumnMapper : IPropertyColumnMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return entityProperty.PropertyType.GetNonNullableType().IsEnum;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            value = ((Enum) value)?.GetValue();

            return new Dictionary<string, object> { { entityProperty.Name, value } };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var type = entityProperty.PropertyType.GetNonNullableType();
            var value = columns.Get(entityProperty.Name);

            return value != null
                ? Enum.ToObject(type, value)
                : type == entityProperty.PropertyType ? (object) 0 : null;
        }

        public string MapToQuery(PropertyInfo entityProperty, object value)
        {
            if (value is Enum @enum)
                value = @enum.GetValue();

            return value.ToString();
        }
    }
}