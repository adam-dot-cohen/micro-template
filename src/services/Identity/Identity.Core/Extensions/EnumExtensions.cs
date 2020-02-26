using System;

namespace Laso.Identity.Core.Extensions
{
    public static class EnumExtensions
    {
        public static object GetValue(this Enum @enum)
        {
            switch (@enum.GetTypeCode())
            {
                case TypeCode.Byte:
                    return Convert.ToByte(@enum);
                case TypeCode.Int16:
                    return Convert.ToInt16(@enum);
                case TypeCode.Int32:
                    return Convert.ToInt32(@enum);
                case TypeCode.Int64:
                    return Convert.ToInt64(@enum);
                case TypeCode.SByte:
                    return Convert.ToSByte(@enum);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(@enum);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(@enum);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(@enum);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
