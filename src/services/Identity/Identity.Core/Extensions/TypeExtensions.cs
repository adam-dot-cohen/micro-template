using System;

namespace Laso.Identity.Core.Extensions
{
    public static class TypeExtensions
    {
        public static Type GetNonNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
