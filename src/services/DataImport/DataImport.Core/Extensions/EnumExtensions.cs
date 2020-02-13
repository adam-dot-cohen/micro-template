using System;

namespace DataImport.Core.Extensions
{
    public static class EnumExtensions
    {
        public static TDest MapByName<TDest>(this Enum value, bool ignoreCase = true) where TDest : struct, Enum
        {
            return Enum.Parse<TDest>(value.ToString(), ignoreCase);
        }
    }
}
