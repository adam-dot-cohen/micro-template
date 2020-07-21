using System;

namespace Laso.IntegrationMessages.AzureStorageQueue.Extensions
{
    internal static class ObjectExtensions
    {
        public static TResult To<T, TResult>(this T instance, Func<T, TResult> transform)
        {
            return transform(instance);
        }

        public static T If<T>(this T instance, Func<T, bool> predicate, Func<T, T> ifTrue)
        {
            return predicate(instance) ? ifTrue(instance) : instance;
        }
    }
}
