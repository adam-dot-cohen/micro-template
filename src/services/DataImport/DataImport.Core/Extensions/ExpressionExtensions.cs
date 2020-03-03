using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Laso.DataImport.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetProperty<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            return GetProperty(expression.Body);
        }

        public static PropertyInfo GetProperty<TValue>(this Expression<Func<TValue>> expression)
        {
            return GetProperty(expression.Body);
        }

        public static PropertyInfo GetProperty(this Expression body)
        {
            var memberExpression = body.NodeType switch
            {
                ExpressionType.Convert => (MemberExpression) ((UnaryExpression) body).Operand,
                ExpressionType.MemberAccess => (MemberExpression) body,
                _ => throw new ArgumentOutOfRangeException()
            };

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
