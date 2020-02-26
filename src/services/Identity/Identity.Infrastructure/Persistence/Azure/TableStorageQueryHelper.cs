using System;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Core.Extensions;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public class TableStorageQueryHelper
    {
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        public TableStorageQueryHelper(IPropertyColumnMapper[] propertyColumnMappers)
        {
            _propertyColumnMappers = propertyColumnMappers;
        }

        public string GetFilter<T>(Expression<Func<T, bool>> filter)
        {
            return GetFilter(filter.Body);
        }

        private string GetFilter(Expression filter)
        {
            switch (filter.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var property = (PropertyInfo) ((MemberExpression) filter).Member;

                    if (property.PropertyType != typeof(bool))
                        throw new NotSupportedException(property.PropertyType.ToString());

                    return $"{property.Name} eq {_propertyColumnMappers.MapToQuery(property, true)}";
                }
                case ExpressionType.Not:
                {
                    var property = (PropertyInfo) ((MemberExpression) ((UnaryExpression) filter).Operand).Member;

                    return $"{property.Name} eq {_propertyColumnMappers.MapToQuery(property, false)}";
                }
                case ExpressionType.Equal:
                    return GetBinaryExpression((BinaryExpression) filter, "eq");
                case ExpressionType.NotEqual:
                    return GetBinaryExpression((BinaryExpression) filter, "ne");
                case ExpressionType.GreaterThan:
                    return GetBinaryExpression((BinaryExpression) filter, "gt");
                case ExpressionType.LessThan:
                    return GetBinaryExpression((BinaryExpression) filter, "lt");
                case ExpressionType.AndAlso:
                    return ((BinaryExpression) filter).To(x => $"({GetFilter(x.Left)} and {GetFilter(x.Right)})");
                case ExpressionType.OrElse:
                    return ((BinaryExpression) filter).To(x => $"({GetFilter(x.Left)} or {GetFilter(x.Right)})");
                case ExpressionType.Constant:
                    var constant = (ConstantExpression) filter;

                    if (constant.Type != typeof(bool))
                        throw new NotSupportedException(constant.Type.ToString());

                    return constant.Value.ToString().ToLower();
                default:
                    throw new ArgumentOutOfRangeException(filter.NodeType.ToString());
            }
        }

        private string GetBinaryExpression(BinaryExpression binaryExpression, string @operator)
        {
            var leftExpression = binaryExpression.Left;
            var rightExpression = binaryExpression.Right;

            if (leftExpression.NodeType == ExpressionType.Convert)
                leftExpression = ((UnaryExpression) leftExpression).Operand;

            if (rightExpression.NodeType == ExpressionType.Convert)
                rightExpression = ((UnaryExpression) rightExpression).Operand;

            if (leftExpression is MemberExpression left && left.Member is PropertyInfo leftProperty)
            {
                var value = Expression.Lambda(rightExpression).Compile().DynamicInvoke();

                return $"{leftProperty.Name} {@operator} {_propertyColumnMappers.MapToQuery(leftProperty, value)}";
            }

            if (rightExpression is MemberExpression right && right.Member is PropertyInfo rightProperty)
            {
                var  value = Expression.Lambda(leftExpression).Compile().DynamicInvoke();

                return $"{_propertyColumnMappers.MapToQuery(rightProperty, value)} {@operator} {rightProperty.Name}";
            }

            throw new NotSupportedException($"Unsupported binary expression: {binaryExpression}");
        }
    }
}
