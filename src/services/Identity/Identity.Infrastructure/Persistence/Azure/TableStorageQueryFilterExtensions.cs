using System;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Infrastructure.Extensions;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public static class TableStorageQueryFilterExtensions
    {
        public static string GetTableStorageFilter<T>(this Expression<Func<T, bool>> filter)
        {
            return GetTableStorageFilter(filter.Body);
        }

        private static string GetTableStorageFilter(Expression filter)
        {
            switch (filter.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var property = (PropertyInfo) ((MemberExpression) filter).Member;

                    if (property.PropertyType != typeof(bool))
                        throw new NotSupportedException(property.PropertyType.ToString());

                    return $"{property.Name} eq true";
                }
                case ExpressionType.Not:
                    return $"{((PropertyInfo) ((MemberExpression) ((UnaryExpression) filter).Operand).Member).Name} eq false";
                case ExpressionType.Equal:
                    return GetBinaryExpression((BinaryExpression) filter, "eq");
                case ExpressionType.NotEqual:
                    return GetBinaryExpression((BinaryExpression) filter, "ne");
                case ExpressionType.GreaterThan:
                    return GetBinaryExpression((BinaryExpression) filter, "gt");
                case ExpressionType.LessThan:
                    return GetBinaryExpression((BinaryExpression) filter, "lt");
                case ExpressionType.AndAlso:
                    return ((BinaryExpression) filter).To(x => $"({GetTableStorageFilter(x.Left)} and {GetTableStorageFilter(x.Right)})");
                case ExpressionType.OrElse:
                    return ((BinaryExpression) filter).To(x => $"({GetTableStorageFilter(x.Left)} or {GetTableStorageFilter(x.Right)})");
                case ExpressionType.Constant:
                    var constant = (ConstantExpression) filter;

                    if (constant.Type != typeof(bool))
                        throw new NotSupportedException(constant.Type.ToString());

                    return constant.Value.ToString().ToLower();
                default:
                    throw new ArgumentOutOfRangeException(filter.NodeType.ToString());
            }
        }

        private static string GetBinaryExpression(BinaryExpression binaryExpression, string @operator)
        {
            if (binaryExpression.Left is MemberExpression left && left.Member is PropertyInfo leftProperty)
            {
                var value = Expression.Lambda(binaryExpression.Right).Compile().DynamicInvoke();

                return $"{leftProperty.Name} {@operator} {PropertyColumnMapper.MapToQuery(leftProperty, value)}";
            }

            if (binaryExpression.Right is MemberExpression right && right.Member is PropertyInfo rightProperty)
            {
                var  value = Expression.Lambda(binaryExpression.Left).Compile().DynamicInvoke();

                return $"{PropertyColumnMapper.MapToQuery(rightProperty, value)} {@operator} {rightProperty.Name}";
            }

            throw new NotSupportedException($"Unsupported binary expression: {binaryExpression}");
        }
    }
}
