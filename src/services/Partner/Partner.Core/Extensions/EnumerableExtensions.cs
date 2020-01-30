using System;
using System.Collections.Generic;
using System.Text;

namespace Partner.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static string AsString(this IEnumerable<char> characters)
        {
            return string.Concat(characters);
        }
    }
}
