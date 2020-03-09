﻿using System;

namespace Laso.Identity.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static TResult To<TSource, TResult>(this TSource source, Func<TSource, TResult> transform)
        {
            return transform(source);
        }

        public static T With<T>(this T instance, Action<T> action)
        {
            action(instance);

            return instance;
        }
    }
}
