using System;
using System.Collections.Generic;
using System.Linq;

namespace Partner.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string AsString(this IEnumerable<char> characters)
        {
            return string.Concat(characters);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || collection.Count() == 0;
        }

        /// <summary>
        /// Executes the specified action on each element in the enumerable.
        /// </summary>        
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {     
            foreach (var item in enumeration)
                action(item);
        }
    }
}
