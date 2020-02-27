using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Laso.Identity.Infrastructure.Extensions
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
            MemberExpression memberExpression;
            switch (body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpression = (MemberExpression) ((UnaryExpression) body).Operand;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression) body;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (PropertyInfo) memberExpression.Member;
        }

        public static MethodInfo GetMethod<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            return expression.GetMethodExpression().Method;
        }

        public static MethodInfo GetMethod<TValue>(this Expression<Func<TValue>> expression)
        {
            return expression.GetMethodExpression().Method;
        }

        public static MethodCallExpression GetMethodExpression<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            return expression.Body.GetMethodExpression();
        }

        public static MethodCallExpression GetMethodExpression<TValue>(this Expression<Func<TValue>> expression)
        {
            return expression.Body.GetMethodExpression();
        }

        public static MethodCallExpression GetMethodExpression(this Expression body)
        {
            MethodCallExpression methodExpression;

            switch (body.NodeType)
            {
                case ExpressionType.Call:
                    methodExpression = (MethodCallExpression)body;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return methodExpression;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter1 = expr1.Parameters[0];

            var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter1);

            var body2WithParam1 = visitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, body2WithParam1), parameter1);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter1 = expr1.Parameters[0];

            var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter1);

            var body2WithParam1 = visitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, body2WithParam1), parameter1);
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