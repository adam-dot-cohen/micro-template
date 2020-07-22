using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Laso.TableStorage.Azure.Extensions
{
    internal static class ExpressionExtensions
    {
        public static PropertyInfo GetProperty<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            return GetProperty(expression.Body);
        }

        public static PropertyInfo GetProperty(this Expression body)
        {
            MemberExpression memberExpression;
            switch (body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpression = (MemberExpression)((UnaryExpression) body).Operand;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression) body;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (PropertyInfo) memberExpression.Member;
        }
    }
}