using System;

namespace Laso.Provisioning.Core.Extensions
{
    public static class TypeExtensions
    {
        public static Type GetNonNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
