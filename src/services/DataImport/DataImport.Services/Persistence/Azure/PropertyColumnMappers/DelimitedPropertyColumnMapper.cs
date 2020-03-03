using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.DataImport.Core.Extensions;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services.Persistence.Azure.PropertyColumnMappers
{
    public class DelimitedPropertyColumnMapper : IPropertyColumnMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            if (entityProperty.GetCustomAttribute<DelimitedAttribute>() == null)
                return false;

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes) && enumTypes[0] == typeof(string))
                return true;

            if (entityProperty.PropertyType.Closes(typeof(IDictionary<,>), out var dictTypes) && dictTypes[0] == typeof(string) && dictTypes[1] == typeof(string))
                return true;

            return false;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            var attribute = entityProperty.GetCustomAttribute<DelimitedAttribute>();

            var mappedValue = entityProperty.PropertyType.Closes(typeof(IDictionary<,>))
                ? value != null ? string.Join(attribute.CollectionDelimiter.ToString(), ((IDictionary<string, string>) value).Select(x => $"{x.Key}{attribute.DictionaryDelimiter}{x.Value}")) : null
                : value != null ? string.Join(attribute.CollectionDelimiter.ToString(), (IEnumerable<string>) value) : null;

            return new Dictionary<string, object> { { entityProperty.Name, mappedValue } };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var attribute = entityProperty.GetCustomAttribute<DelimitedAttribute>();

            var value = (string) columns.Get(entityProperty.Name);

            return entityProperty.PropertyType.Closes(typeof(IDictionary<,>))
                ? value != null ? value.Split(attribute.CollectionDelimiter).Select(y => y.Split(attribute.DictionaryDelimiter)).ToDictionary(y => y[0], y => y[1]) : new Dictionary<string, string>()
                : value != null ? (object) value.Split(attribute.CollectionDelimiter) : new List<string>();
        }

        public string MapToQuery(PropertyInfo entityProperty, object value)
        {
            throw new NotSupportedException();
        }
    }
}