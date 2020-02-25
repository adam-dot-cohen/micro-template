using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;

namespace Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers
{
    public class EnumPropertyColumnMapper : IPropertyColumnMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return entityProperty.PropertyType.GetNonNullableType().IsEnum;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            return new Dictionary<string, object> { { entityProperty.Name, value } };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var type = entityProperty.PropertyType.GetNonNullableType();
            var value = (string) columns.Get(entityProperty.Name);

            return value != null
                ? Enum.Parse(type, value)
                : type == entityProperty.PropertyType
                    ? Enum.GetValues(type).Cast<object>().First()
                    : null;
        }

        public string MapToQuery(PropertyInfo entityProperty, object value)
        {
            throw new NotImplementedException();
        }
    }
}