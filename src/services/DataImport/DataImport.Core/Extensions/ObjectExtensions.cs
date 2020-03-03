using System;
using System.Linq.Expressions;

namespace Laso.DataImport.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static T ConvertTo<T>(this object value)
        {
            if (value is T result)
                return result;

            var type = typeof(T);

            if (type == typeof(Guid))
                return (T)(object)Guid.Parse(value.ConvertTo<string>());

            return (T)Convert.ChangeType(value, type);
        }

        public static void SetValue<TSource, TValue>(this TSource obj, Expression<Func<TSource, TValue>> expression, TValue value)
        {
            expression.GetProperty().SetValue(obj, value);
        }
    }
}
