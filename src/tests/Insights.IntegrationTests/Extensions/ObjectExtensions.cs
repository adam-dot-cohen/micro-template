using System;

namespace Laso.Insights.IntegrationTests.Extensions
{
    internal static class ObjectExtensions
    {
        public static TResult To<T, TResult>(this T instance, Func<T, TResult> transform)
        {
            return transform(instance);
        }
    }
}