using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Persistence.Azure
{
    public class TableStorageQueryHelperTests
    {
        private static readonly DateTime RushTime = new DateTime(2112, 12, 21);

        public static TheoryData<Expression<Func<TestEntity, bool>>, string> FilterTestCases = new TheoryData<Expression<Func<TestEntity, bool>>, string>
        {
            {x => true, "true"},
            {x => false, "false"},

            {x => x.Boolean, "Boolean eq true"},
            {x => !x.Boolean, "Boolean eq false"},

            {x => x.Integer == 2112, "Integer eq 2112"},
            {x => 2112 == x.Integer, "2112 eq Integer"},
            {x => x.Integer != 2112, "Integer ne 2112"},
            {x => 2112 != x.Integer, "2112 ne Integer"},
            {x => x.Integer > 2112, "Integer gt 2112"},
            {x => 2112 > x.Integer, "2112 gt Integer"},
            {x => x.Integer < 2112, "Integer lt 2112"},
            {x => 2112 < x.Integer, "2112 lt Integer"},

            {x => (x.String == "Rush" || x.Integer == 2112 && (x.Boolean || false)) && x.DateTime == RushTime, "((String eq 'Rush' or (Integer eq 2112 and (Boolean eq true or false))) and DateTime eq datetime'2112-12-21T00:00:00Z')" },
        };

        [Theory, MemberData(nameof(FilterTestCases))]
        public void Should_generate_query_filter(Expression<Func<TestEntity, bool>> expression, string filter)
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(expression).ShouldBe(filter);
        }

        [Fact]
        public void Should_support_local_variables()
        {
            var value = "Rush";

            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter<TestEntity>(x => x.String == value).ShouldBe("String eq 'Rush'");
        }

        [Fact]
        public void Should_support_fields()
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter<TestEntity>(x => x.DateTime == RushTime).ShouldBe("DateTime eq datetime'2112-12-21T00:00:00Z'");
        }

        [Fact]
        public void Should_support_properties()
        {
            var entity = new TestEntity { Integer = 2112 };

            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter<TestEntity>(x => x.Integer == entity.Integer).ShouldBe("Integer eq 2112");
        }

        [Fact]
        public void Should_support_concatenated_expressions()
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            helper.GetFilter(GetExpression(x => true).And(x => x.Boolean)).ShouldBe("(true and Boolean eq true)");
        }

        [Fact]
        public void Should_select_single_property()
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            var (select, project) = helper.GetSelect<TestEntity, int>(x => x.Integer);

            select.Single().ShouldBe("Integer");
            project(new TestEntity { Integer = 2112 }).ShouldBe(2112);
        }

        [Fact]
        public void Should_select_projection()
        {
            var helper = new TableStorageQueryHelper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });

            var (select, project) = helper.GetSelect(GetExpression(x => new { x.Boolean, Name = x.String, RushTime = x.DateTime.ToString("d") }));

            select.ShouldContain("Boolean");
            select.ShouldContain("String");
            select.ShouldContain("DateTime");
            var projection = project(new TestEntity { Boolean = true, String = "Rush", DateTime = RushTime });
            projection.Boolean.ShouldBeTrue();
            projection.Name.ShouldBe("Rush");
            projection.RushTime.ShouldBe("12/21/2112");
        }

        private static Expression<Func<TestEntity, TResult>> GetExpression<TResult>(Expression<Func<TestEntity, TResult>> expression)
        {
            return expression;
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
