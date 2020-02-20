using System;

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
    }
}
