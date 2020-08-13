using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.Filters;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure.PropertyColumnMappers
{
    public class ComponentPropertyColumnMapper : IPropertyColumnMapper
    {
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        public ComponentPropertyColumnMapper(IPropertyColumnMapper[] propertyColumnMappers)
        {
            _propertyColumnMappers = propertyColumnMappers;
        }

        public bool CanMap(PropertyInfo entityProperty)
        {
            if (entityProperty.GetCustomAttribute<ComponentAttribute>() == null && entityProperty.PropertyType.GetCustomAttribute<ComponentAttribute>() == null)
                return false;

            if (!entityProperty.PropertyType.IsClass) //TODO: support structs?
                return false;

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>)))
                return false;

            return true;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            var prefix = entityProperty.Name + "_";

            return entityProperty.PropertyType
                .GetProperties()
                .SelectMany(x => _propertyColumnMappers.MapToColumns(x, value != null ? x.GetValue(value) : null))
                .ToDictionary(x => prefix + x.Key, x => x.Value);
        }

        public ICollection<string> MapToColumns(PropertyInfo entityProperty)
        {
            var prefix = entityProperty.Name + "_";

            return entityProperty.PropertyType
                .GetProperties()
                .SelectMany(x => _propertyColumnMappers.MapToColumns(x))
                .Select(x => prefix + x)
                .ToList();
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var entity = Activator.CreateInstance(entityProperty.PropertyType, true);
            var prefix = entityProperty.Name + "_";
            columns = columns.Where(y => y.Key.StartsWith(prefix)).ToDictionary(x => x.Key.Substring(prefix.Length), x => x.Value);

            entityProperty.PropertyType
                .GetProperties()
                .Where(x => x.CanWrite)
                .ForEach(x => x.SetValue(entity, _propertyColumnMappers.MapToProperty(x, columns)));

            return entity;
        }

        public string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            throw new NotSupportedException();
        }
    }
}
