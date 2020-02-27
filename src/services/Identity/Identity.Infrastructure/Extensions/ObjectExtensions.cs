﻿using System;
using System.Linq.Expressions;

namespace Laso.Identity.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static void SetValue<TSource, TValue>(this TSource obj, Expression<Func<TSource, TValue>> expression, TValue value)
        {
            expression.GetProperty().SetValue(obj, value);
        }
    }
}
