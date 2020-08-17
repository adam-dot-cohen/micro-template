using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laso.Filters;
using Laso.TableStorage.Azure.Extensions;
using Laso.TableStorage.Domain;

namespace Laso.TableStorage.Azure.PropertyColumnMappers
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
                && IsAcceptableType(dictTypes.First()[0])
                && IsAcceptableType(dictTypes.First()[1]))
                return true;

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes)
                && IsAcceptableType(enumTypes.First()[0]))
                return true;

            return false;
        }

        public IDictionary<string, object> MapToColumns(PropertyInfo entityProperty, object value)
        {
            var attribute = entityProperty.GetCustomAttribute<DelimitedAttribute>();

            string mappedValue;
            if (entityProperty.PropertyType.Closes(typeof(IDictionary<,>), out var dictTypes))
            {
                var args = dictTypes.First();

                mappedValue = ((IDictionary) value)?.ToEnumerable()
                    .Select(x => $"{ToString(x.Key, args[0])}{attribute.DictionaryDelimiter}{ToString(x.Value, args[1])}")
                    .Join(attribute.CollectionDelimiter.ToString());
            }
            else if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes))
            {
                var args = enumTypes.First();

                mappedValue = ((IEnumerable) value)?.Cast<object>()
                    .Select(x => ToString(x, args[0]))
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
                var args = dictTypes.First();

                var dictionary = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(args));

                value?.Split(attribute.CollectionDelimiter)
                    .Select(x => x.Split(attribute.DictionaryDelimiter))
                    .ForEach(x => dictionary.Add(Parse(x[0], args[0]), Parse(x[1], args[1])));

                return dictionary;
            }

            if (entityProperty.PropertyType.Closes(typeof(IEnumerable<>), out var enumTypes))
            {
                var args = enumTypes.First();

                var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(args));

                value?.Split(attribute.CollectionDelimiter).ForEach(x => list.Add(Parse(x, args[0])));

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