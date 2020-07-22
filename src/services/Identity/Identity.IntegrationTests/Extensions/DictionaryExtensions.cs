using System.Collections.Generic;

namespace Laso.Identity.IntegrationTests.Extensions
{
    internal static class DictionaryExtensions
    {
        public static TResult Get<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }
    }
}