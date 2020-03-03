using System;
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

        public static TheoryData<Expression<Func<TestEntity, bool>>, string> TestCases = new TheoryData<Expression<Func<TestEntity, bool>>, string>
        {
            {x => true, "true"},
            {x => false, "false"},

            {x => x.Boolean, "Boolean eq true"},
            {x => !x.Boolean, "Boolean eq false"},

            {x => (x.String == "Rush" || x.Integer == 2112 && (x.Boolean || false)) && x.DateTime == RushTime, "((String eq 'Rush' or (Integer eq 2112 and (Boolean eq true or false))) and DateTime eq datetime'2112-12-21T00:00:00Z')" },
        };

        [Theory, MemberData(nameof(TestCases))]
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

            helper.GetFilter(((Expression<Func<TestEntity, bool>>) (x => true)).And(x => x.Boolean)).ShouldBe("(true and Boolean eq true)");
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
