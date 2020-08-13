using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Laso.TableStorage.Tests.Extensions
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
                    memberExpression = (MemberExpression)((UnaryExpression)body).Operand;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression)body;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (PropertyInfo)memberExpression.Member;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter1 = expr1.Parameters[0];

            var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter1);

            var body2WithParam1 = visitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, body2WithParam1), parameter1);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (ReferenceEquals(node, _oldParameter))
                    return _newParameter;

                return base.VisitParameter(node);
            }
        }
    }
}
