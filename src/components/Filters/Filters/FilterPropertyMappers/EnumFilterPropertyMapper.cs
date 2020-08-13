using System;
using System.Reflection;
using Laso.Filters.Extensions;

namespace Laso.Filters.FilterPropertyMappers
{
    public class EnumFilterPropertyMapper : IFilterPropertyMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return entityProperty.PropertyType.GetNonNullableType().IsEnum;
        }

        public string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            if (value is Enum @enum)
                return dialect.GetEnumParameter(@enum);

            return dialect.GetPrimitiveParameter(value);
        }
    }
}