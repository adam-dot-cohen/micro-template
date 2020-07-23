using System;
using System.Collections.Generic;

namespace Laso.Identity.Infrastructure.Extensions
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        ///     Executes the specified action on each element in the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }
    }
}