using System;

namespace Laso.DataImport.Core.Extensions
{
    public static class EnumExtensions
    {
        public static TDest MapByName<TDest>(this Enum value, bool ignoreCase = true) where TDest : struct, Enum
        {
            if (!Enum.TryParse(value.ToString(), ignoreCase, out TDest parsed))
                throw new Exception($"Unable to convert {value.ToString()} to destination type");

            return parsed;
        }
    }
}
