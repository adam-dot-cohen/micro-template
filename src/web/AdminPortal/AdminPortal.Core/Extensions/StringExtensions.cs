﻿using System.Collections.Generic;

namespace Laso.AdminPortal.Core.Extensions
{
    internal static class StringExtensions
    {
        public static string Join(this IEnumerable<string> input, string separator)
        {
            return string.Join(separator, input);
        }
    }
}
