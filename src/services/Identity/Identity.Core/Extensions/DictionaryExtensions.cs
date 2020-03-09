using System.Collections.Generic;
using System.Linq;

namespace Laso.Identity.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static TResult Get<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key, TResult defaultValue = default)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }

        public static IEnumerable<(TValue Left, TValue Right)> LeftJoin<T, TValue>(this IDictionary<T, TValue> left, IDictionary<T, TValue> right)
        {
            return left.Select(y => (y.Value, right.Get(y.Key)));
        }
    }
}