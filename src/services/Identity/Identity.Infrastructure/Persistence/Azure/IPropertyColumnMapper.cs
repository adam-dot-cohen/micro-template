using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public interface IPropertyColumnMapper
    {
        bool CanMap(PropertyInfo entityProperty);
        IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value);
        ICollection<string> MapToColumns(PropertyInfo entityProperty);
        object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns);
        string MapToQuery(PropertyInfo entityProperty, object value);
    }

    public static class PropertyColumnMapper
    {
        public static IDictionary<string, object> MapToColumns(this IEnumerable<IPropertyColumnMapper> mappers, PropertyInfo entityProperty, object value)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToColumns(entityProperty, value);
        }

        public static ICollection<string> MapToColumns(this IEnumerable<IPropertyColumnMapper> mappers, PropertyInfo entityProperty)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToColumns(entityProperty);
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
