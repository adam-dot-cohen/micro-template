using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public interface IPropertyColumnMapper
    {
        bool CanMap(PropertyInfo entityProperty);
        IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value);
        object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns);
        string MapToQuery(PropertyInfo entityProperty, object value);
    }

    public static class PropertyColumnMapper
    {
        private static readonly IPropertyColumnMapper[] Mappers =
        {
            new EnumPropertyColumnMapper(),
            new DelimitedPropertyColumnMapper(),
            new DefaultPropertyColumnMapper()
        };

        public static IDictionary<string, object> MapToColumns(this IEnumerable<IPropertyColumnMapper> mappers, PropertyInfo entityProperty, object value)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToColumns(entityProperty, value);
        }

        public static object MapToProperty(this IEnumerable<IPropertyColumnMapper> mappers, PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToProperty(entityProperty, columns);
        }

        public static string MapToQuery(this IEnumerable<IPropertyColumnMapper> mappers, PropertyInfo entityProperty, object value)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToQuery(entityProperty, value);
        }
    }
}
