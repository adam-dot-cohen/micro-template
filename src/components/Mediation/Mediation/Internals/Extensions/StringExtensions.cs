using System.Collections.Generic;

namespace Infrastructure.Mediation.Internals.Extensions
{
    internal static class StringExtensions
    {
        internal static string Join(this IEnumerable<string> input, string separator)
        {
            return string.Join(separator, input);
        }
    }
}