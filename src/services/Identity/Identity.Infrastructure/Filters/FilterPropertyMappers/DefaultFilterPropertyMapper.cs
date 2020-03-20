using System;
using System.Reflection;
using Laso.Identity.Infrastructure.Extensions;

namespace Laso.Identity.Infrastructure.Filters.FilterPropertyMappers
{
    public class DefaultFilterPropertyMapper : IFilterPropertyMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return true;
        }

        public string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            if (value == null)
                return null;

            var type = entityProperty.PropertyType.GetNonNullableType();

            if (type == typeof(bool))
                return dialect.GetBooleanParameter((bool) value);
            if (type == typeof(string))
                return dialect.GetStringParameter((string) value);
            if (type == typeof(DateTime))
                return dialect.GetDateTimeParameter((DateTime) value);
            if (type == typeof(Guid))
                return dialect.GetGuidParameter((Guid) value);
            if (type.IsPrimitive)
                return dialect.GetPrimitiveParameter(value);

            throw new ArgumentOutOfRangeException(type.Name);
        }
    }
}