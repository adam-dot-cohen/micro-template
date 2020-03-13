using System;

namespace Laso.Identity.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static TResult To<T, TResult>(this T instance, Func<T, TResult> transform)
        {
            return transform(instance);
        }

        public static T With<T>(this T instance, Action<T> action)
        {
            action(instance);

            return instance;
        }

        public static T If<T>(this T instance, Func<T, bool> predicate, Func<T, T> ifTrue)
        {
            return predicate(instance) ? ifTrue(instance) : instance;
        }

        public static TResult If<T, TResult>(this T instance, Func<T, bool> predicate, Func<T, TResult> ifTrue, Func<T, TResult> ifFalse)
        {
            return predicate(instance) ? ifTrue(instance) : ifFalse(instance);
        }
    }
}
