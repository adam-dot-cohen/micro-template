using System;
using System.Reflection;
using Laso.AdminPortal.Infrastructure.Extensions;

namespace Laso.AdminPortal.Infrastructure.Filters.FilterPropertyMappers
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
                return dialect.GetBoolean((bool) value);
            if (type == typeof(string))
                return dialect.GetString((string) value);
            if (type == typeof(DateTime))
                return dialect.GetDateTime((DateTime) value);
            if (type == typeof(Guid))
                return dialect.GetGuid((Guid) value);
            if (type.IsPrimitive)
                return dialect.GetPrimitive(value);

            throw new ArgumentOutOfRangeException(type.Name);
        }
    }
}