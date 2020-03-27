using System;
using System.Collections.Generic;
using System.Reflection;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Filters.FilterPropertyMappers;

namespace Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers
{
    public class EnumPropertyColumnMapper : EnumFilterPropertyMapper, IPropertyColumnMapper
    {
        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            value = ((Enum) value)?.GetValue();

            return new Dictionary<string, object> { { entityProperty.Name, value } };
        }

        public ICollection<string> MapToColumns(PropertyInfo entityProperty)
        {
            return new[] { entityProperty.Name };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var type = entityProperty.PropertyType.GetNonNullableType();
            var value = columns.Get(entityProperty.Name);

            return value != null
                ? Enum.ToObject(type, value)
                : type == entityProperty.PropertyType ? (object) 0 : null;
        }
    }
}