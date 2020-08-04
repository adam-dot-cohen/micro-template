using System.Collections.Generic;

namespace IntegrationMessages.AzureServiceBus.Extensions
{
    internal static class EnumerableExtensions
    {
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
    }
}
