using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.Insights.IntegrationTests.Extensions
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

        public static IEnumerable<T> Concat<T>(this T item, IEnumerable<T> collection)
        {
            yield return item;

            foreach (var x in collection)
                yield return x;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
        {
            foreach (var x in source)
                yield return x;

            yield return item;
        }

        public static IEnumerable<T> Distinct<T, TValue>(this IEnumerable<T> enumerable, Func<T, TValue> getEqualityOperand)
        {
            return enumerable.ToLookup(getEqualityOperand).Select(x => x.First());
        }
    }
}
