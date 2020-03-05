using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Laso.DataImport.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string AsString(this IEnumerable<char> characters)
        {
            return string.Concat(characters);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Executes the specified action on each element in the enumerable.
        /// </summary>        
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {     
            foreach (var item in enumeration)
                action(item);
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

        public static IEnumerable<DictionaryEntry> ToEnumerable(this IDictionary dictionary)
        {
            return dictionary.Cast<DictionaryEntry>();
        }
    }
}
