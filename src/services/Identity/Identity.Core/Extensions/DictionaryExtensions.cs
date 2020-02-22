using System.Collections.Generic;

namespace Laso.Identity.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static TResult Get<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default(TResult))
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }
    }
}