using System;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Persistence
{
    public class TableStorageQueryHelperTests
    {
        private static readonly DateTime RushTime = new DateTime(2112, 12, 21);

        public static TheoryData<Expression<Func<TestEntity, bool>>, string> TestCases = new TheoryData<Expression<Func<TestEntity, bool>>, string>
        {
            {x => true, "true"},
            {x => false, "false"},

            {x => x.Boolean, "Boolean eq true"},
            {x => !x.Boolean, "Boolean eq false"},

            {x => (x.String == "Rush" || x.Integer == 2112 && (x.Boolean || false)) && x.DateTime == RushTime, "((String eq 'Rush' or (Integer eq 2112 and (Boolean eq true or false))) and DateTime eq datetime'2112-12-21T00:00:00Z')" },
            { GetExpression(x => false).And(x => x.Boolean), "(false and Boolean eq true)" }
        };

        [Theory, MemberData(nameof(TestCases))]
        public void Should_generate_query_filter(Expression<Func<TestEntity, bool>> expression, string filter)
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(expression).ShouldBe(filter);
        }

        [Fact]
        public void Should_work_with_local_variable()
        {
            var value = "Rush";

            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(GetExpression(x => x.String == value)).ShouldBe("String eq 'Rush'");
        }

        [Fact]
        public void Should_work_with_field()
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(GetExpression(x => x.DateTime == RushTime)).ShouldBe("DateTime eq datetime'2112-12-21T00:00:00Z'");
        }

        [Fact]
        public void Should_work_property()
        {
            var entity = new TestEntity { Integer = 2112 };

            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(GetExpression(x => x.Integer == entity.Integer)).ShouldBe("Integer eq 2112");
        }

        private static Expression<Func<TestEntity, bool>> GetExpression(Expression<Func<TestEntity, bool>> expression)
        {
            return expression;
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<TestEntity, T>> expression)
        {
            return expression.GetProperty();
        }

        public class TestEntity
        {
            public bool Boolean { get; set; }
            public int Integer { get; set; }
            public string String { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
