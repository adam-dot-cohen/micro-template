using System;
using System.Diagnostics.CodeAnalysis;

namespace Insights.AccountTransactionClassifier.Function.Extensions
{
    public static class ObjectExtensions
    {
        [return: MaybeNull]
        public static T ConvertTo<T>(this object? value)
        {
            if (value == null)
                return default!;

            if (value is T result)
                return result;

            var type = typeof(T);

            //if (type.IsEnumeration())
            //    return (T)Enumeration.FromValue(type, value);

            //if (type.IsSimpleEnum())
            //    return (T)SimpleEnum.FromValue(type, value);

            if (type == typeof(Guid))
            {
                var guidString = value.ConvertTo<string>();
                if (guidString == null)
                    return default!;

                return (T)(object)Guid.Parse(guidString);
            }

            return (T)Convert.ChangeType(value, type);
        }
    }
}
