using System;
using System.Linq.Expressions;

namespace Laso.Provisioning.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void SetValue<T, TValue>(this T obj, Expression<Func<T, TValue>> expression, TValue value)
        {
            expression.GetProperty().SetValue(obj, value);
        }
    }
}
