using System.Collections.Generic;

namespace Laso.AdminPortal.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (value != null && value.Length > maxLength)
                value = value.Substring(0, maxLength);

            return value;
        }

        public static string Join(this IEnumerable<string> input, string separator)
        {
            return string.Join(separator, input);
        }
    }
}
