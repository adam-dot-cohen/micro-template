using System;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Core.Extensions;

namespace Laso.Identity.Infrastructure.Filters
{
    public class FilterExpressionHelper
    {
        private readonly IFilterPropertyMapper[] _filterPropertyMappers;
        private readonly IFilterDialect _dialect;

        public FilterExpressionHelper(IFilterPropertyMapper[] filterPropertyMappers, IFilterDialect dialect)
        {
            _filterPropertyMappers = filterPropertyMappers;
            _dialect = dialect;
        }

        public string GetFilter<T>(Expression<Func<T, bool>> filter)
        {
            return GetFilter((LambdaExpression)filter);
        }

        public string GetFilter(LambdaExpression filter)
        {
            if (filter == null)
                return null;

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

                    return $"{property.Name} {_dialect.Equal} {_filterPropertyMappers.MapToQueryParameter(_dialect, property, true)}";
                }
                case ExpressionType.Not:
                {
                    var property = (PropertyInfo) ((MemberExpression) ((UnaryExpression) filter).Operand).Member;

                    return $"{property.Name} eq {_filterPropertyMappers.MapToQueryParameter(_dialect, property, false)}";
                }
                case ExpressionType.Equal:
                    return GetBinaryExpression((BinaryExpression) filter, _dialect.Equal);
                case ExpressionType.NotEqual:
                    return GetBinaryExpression((BinaryExpression) filter, _dialect.NotEqual);
                case ExpressionType.GreaterThan:
                    return GetBinaryExpression((BinaryExpression) filter, _dialect.GreaterThan);
                case ExpressionType.LessThan:
                    return GetBinaryExpression((BinaryExpression) filter, _dialect.LessThan);
                case ExpressionType.GreaterThanOrEqual:
                    return GetBinaryExpression((BinaryExpression)filter, _dialect.GreaterThanOrEqual);
                case ExpressionType.LessThanOrEqual:
                    return GetBinaryExpression((BinaryExpression)filter, _dialect.LessThanOrEqual);
                case ExpressionType.AndAlso:
                    return ((BinaryExpression) filter).To(x => $"({GetFilter(x.Left)} {_dialect.And} {GetFilter(x.Right)})");
                case ExpressionType.OrElse:
                    return ((BinaryExpression) filter).To(x => $"({GetFilter(x.Left)} {_dialect.Or} {GetFilter(x.Right)})");
                case ExpressionType.Constant:
                    var constant = (ConstantExpression) filter;

                    if (constant.Type != typeof(bool))
                        throw new NotSupportedException(constant.Type.ToString());

                    return constant.Value.ToString().ToLower();
                default:
                    throw new ArgumentOutOfRangeException(filter.NodeType.ToString(), filter.ToString());
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

                return $"{leftProperty.Name} {@operator} {_filterPropertyMappers.MapToQueryParameter(_dialect, leftProperty, value)}";
            }

            if (rightExpression is MemberExpression right && right.Member is PropertyInfo rightProperty)
            {
                var  value = Expression.Lambda(leftExpression).Compile().DynamicInvoke();

                return $"{_filterPropertyMappers.MapToQueryParameter(_dialect, rightProperty, value)} {@operator} {rightProperty.Name}";
            }

            throw new NotSupportedException($"Unsupported binary expression: {binaryExpression}");
        }
    }
}
