using System.Collections.Generic;
using System.Reflection;
using Laso.Filters.FilterPropertyMappers;
using Laso.TableStorage.Azure.Extensions;

namespace Laso.TableStorage.Azure.PropertyColumnMappers
{
    public class DefaultPropertyColumnMapper : DefaultFilterPropertyMapper, IPropertyColumnMapper
    {
        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            return new Dictionary<string, object> { { entityProperty.Name, value } };
        }

        public ICollection<string> MapToColumns(PropertyInfo entityProperty)
        {
            return new[] { entityProperty.Name };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            return columns.Get(entityProperty.Name);
        }
    }
}