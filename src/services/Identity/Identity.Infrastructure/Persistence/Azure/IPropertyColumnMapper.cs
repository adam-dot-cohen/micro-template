using System.Collections.Generic;
using System.Reflection;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public interface IPropertyColumnMapper
    {
        bool CanMap(PropertyInfo entityProperty);
        IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value);
        object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns);
    }

    public static class PropertyColumnMapper
    {
        public static IPropertyColumnMapper[] GetMappers()
        {
            return new IPropertyColumnMapper[]
            {
                new EnumPropertyColumnMapper(),
                new DelimitedPropertyColumnMapper(),
                new DefaultPropertyColumnMapper()
            };
        }
    }
}
