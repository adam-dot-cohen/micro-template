using System;
using System.Collections.Generic;

namespace Laso.Provisioning.Core.Extensions
{
    public static class EnumerableExtensions
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

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            var nextBatch = new List<T>();

            foreach (var item in collection)
            {
                nextBatch.Add(item);
                if (nextBatch.Count != batchSize)
                    continue;

                yield return nextBatch;
                nextBatch = new List<T>(batchSize);
            }

            if (nextBatch.Count > 0)
                yield return nextBatch;
        }
    }
}
