using System;
using System.Collections.Generic;

namespace Insights.AccountTransactionClassifier.Function.Extensions
{
    public static class StringExtensions
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

        /// <summary>
        ///     Executes the specified action on each element in the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T, int> action)
        {
            var i = 0;

            foreach (var item in enumeration)
                action(item, i++);
        }
    }
}
