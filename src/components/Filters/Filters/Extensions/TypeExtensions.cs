using System;

namespace Laso.Filters.Extensions
{
    internal static class TypeExtensions
    {
        public static Type GetNonNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
