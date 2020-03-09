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

        public static object GetValue(this Enum @enum)
        {
            return @enum.GetTypeCode() switch
            {
                TypeCode.Byte => (object) Convert.ToByte(@enum),
                TypeCode.Int16 => Convert.ToInt16(@enum),
                TypeCode.Int32 => Convert.ToInt32(@enum),
                TypeCode.Int64 => Convert.ToInt64(@enum),
                TypeCode.SByte => Convert.ToSByte(@enum),
                TypeCode.UInt16 => Convert.ToUInt16(@enum),
                TypeCode.UInt32 => Convert.ToUInt32(@enum),
                TypeCode.UInt64 => Convert.ToUInt64(@enum),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
