using System.Collections.Generic;

namespace DataImport.Core.Extensions
{
    public static class IDictionaryExtensions
    {
        public static TResult Get<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var result) ? result : defaultValue;
        }

        public static TResult Get<TKey, TResult>(this IReadOnlyDictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default(TResult))
        {
            return dictionary.TryGetValue(key, out var result) ? result : defaultValue;
        }

        public static TResult Get<TKey, TResult>(this Dictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default(TResult))
        {
            return dictionary.TryGetValue(key, out var result) ? result : defaultValue;
        }
    }
}
