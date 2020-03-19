using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laso.AdminPortal.Infrastructure.Filters
{
    public interface IFilterPropertyMapper
    {
        bool CanMap(PropertyInfo entityProperty);
        string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value);
    }

    public static class FilterPropertyMapper
    {
        public static string MapToQueryParameter(this IEnumerable<IFilterPropertyMapper> mappers, IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            return mappers.First(y => y.CanMap(entityProperty)).MapToQueryParameter(dialect, entityProperty, value);
        }
    }
}