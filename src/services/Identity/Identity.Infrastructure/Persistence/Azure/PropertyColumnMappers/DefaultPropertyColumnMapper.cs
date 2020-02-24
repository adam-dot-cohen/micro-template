using System.Collections.Generic;
using System.Reflection;
using Laso.Identity.Core.Extensions;

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
    }
}