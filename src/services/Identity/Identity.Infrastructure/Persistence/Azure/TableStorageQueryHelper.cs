using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Infrastructure.Filters;

namespace Laso.Identity.Infrastructure.Persistence.Azure
{
    public class TableStorageQueryHelper : FilterExpressionHelper
    {
        private readonly IPropertyColumnMapper[] _propertyColumnMappers;

        public TableStorageQueryHelper(IPropertyColumnMapper[] propertyColumnMappers) : base(
            propertyColumnMappers.Cast<IFilterPropertyMapper>().ToArray(),
            new TableStorageFilterDialect())
        {
            _propertyColumnMappers = propertyColumnMappers;
        }

        public (IList<string> Select, Func<T, TResult> Project) GetSelect<T, TResult>(Expression<Func<T, TResult>> select)
        {
            if (select == null)
                return (null, null);

            var visitor = new EntityPropertyExpressionVisitor<T>();
            visitor.Visit(select);

            return (visitor.Properties.SelectMany(x => _propertyColumnMappers.MapToColumns(x)).ToList(), select.Compile());
        }

        private class EntityPropertyExpressionVisitor<T> : ExpressionVisitor
        {
            public HashSet<PropertyInfo> Properties { get; } = new HashSet<PropertyInfo>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == typeof(T))
                    Properties.Add((PropertyInfo) node.Member);

                return base.VisitMember(node);
            }
        }
    }
}
