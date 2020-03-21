using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Filters;

namespace Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers
{
    public class DelimitedPropertyColumnMapper : IPropertyColumnMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            if (entityProperty.GetCustomAttribute<DelimitedAttribute>() == null)
                return false;

            bool IsAcceptableType(Type type)
            {
                return type == typeof(string) || type.IsEnum || type.IsPrimitive;
            }

            if (entityProperty.PropertyType.Closes(typeof(IDictionary<,>), out var dictTypes)
                && IsAcceptableType(dictTypes[0])
                && IsAcceptableType(dictTypes[1]))
                return true;

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes)
                && IsAcceptableType(enumTypes[0]))
                return true;

            return false;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            var attribute = entityProperty.GetCustomAttribute<DelimitedAttribute>();

            string mappedValue;
            if (entityProperty.PropertyType.Closes(typeof(IDictionary<,>), out var dictTypes))
            {
                mappedValue = ((IDictionary) value)?.ToEnumerable()
                    .Select(x => $"{ToString(x.Key, dictTypes[0])}{attribute.DictionaryDelimiter}{ToString(x.Value, dictTypes[1])}")
                    .Join(attribute.CollectionDelimiter.ToString());
            }
            else if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes))
            {
                mappedValue = ((IEnumerable) value)?.Cast<object>()
                    .Select(x => ToString(x, enumTypes[0]))
                    .Join(attribute.CollectionDelimiter.ToString());
            }
            else
            {
                throw new NotSupportedException(entityProperty.PropertyType.Name);
            }

            return new Dictionary<string, object> { { entityProperty.Name, mappedValue } };
        }

        public ICollection<string> MapToColumns(PropertyInfo entityProperty)
        {
            return new[] { entityProperty.Name };
        }

        public object MapToProperty(PropertyInfo entityProperty, IDictionary<string, object> columns)
        {
            var attribute = entityProperty.GetCustomAttribute<DelimitedAttribute>();

            var value = (string) columns.Get(entityProperty.Name);

            if (entityProperty.PropertyType.Closes(typeof(IDictionary<,>), out var dictTypes))
            {
                var dictionary = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(dictTypes));

                value?.Split(attribute.CollectionDelimiter)
                    .Select(x => x.Split(attribute.DictionaryDelimiter))
                    .ForEach(x => dictionary.Add(Parse(x[0], dictTypes[0]), Parse(x[1], dictTypes[1])));

                return dictionary;
            }

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes))
            {
                var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(enumTypes));

                value?.Split(attribute.CollectionDelimiter).ForEach(x => list.Add(Parse(x, enumTypes[0])));

                return list;
            }

            throw new NotSupportedException(entityProperty.PropertyType.Name);
        }

        public string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            throw new NotSupportedException();
        }

        private static string ToString(object value, Type type)
        {
            if (type.IsEnum)
                return ((Enum) value)?.GetValue().ToString();

            return value.ToString();
        }

        private static object Parse(string value, Type type)
        {
            if (type.IsEnum)
                return Enum.Parse(type, value);

            return Convert.ChangeType(value, type);
        }
    }
}