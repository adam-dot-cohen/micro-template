using System;
using System.Linq.Expressions;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Persistence
{
    public class TableStorageQueryFilterExtensionsTests
    {
        private static readonly DateTime RushTime = new DateTime(2112, 12, 21);
        private static readonly Guid RushId = Guid.NewGuid();

        public static TheoryData<Expression<Func<TestEntity, bool>>, string> TestCases = new TheoryData<Expression<Func<TestEntity, bool>>, string>
        {
            {x => true, "true"},
            {x => false, "false"},

            {x => x.Boolean, "Boolean eq true"},
            {x => !x.Boolean, "Boolean eq false"},
            {x => x.Boolean == true, "Boolean eq true"},
            {x => x.Boolean == false, "Boolean eq false"},
            {x => true == x.Boolean, "true eq Boolean"},
            {x => false == x.Boolean, "false eq Boolean"},

            {x => x.Integer == 2112, "Integer eq 2112"},
            {x => 2112 == x.Integer, "2112 eq Integer"},
            {x => x.Integer != 2112, "Integer ne 2112"},
            {x => 2112 != x.Integer, "2112 ne Integer"},
            {x => x.Integer > 2112, "Integer gt 2112"},
            {x => 2112 > x.Integer, "2112 gt Integer"},
            {x => x.Integer < 2112, "Integer lt 2112"},
            {x => 2112 < x.Integer, "2112 lt Integer"},

            {x => x.Nullable == 2112, "Nullable eq 2112"},
            {x => 2112 == x.Nullable, "2112 eq Nullable"},
            {x => x.Nullable != 2112, "Nullable ne 2112"},
            {x => 2112 != x.Nullable, "2112 ne Nullable"},
            {x => x.Nullable > 2112, "Nullable gt 2112"},
            {x => 2112 > x.Nullable, "2112 gt Nullable"},
            {x => x.Nullable < 2112, "Nullable lt 2112"},
            {x => 2112 < x.Nullable, "2112 lt Nullable"},

            {x => x.String == "Rush", "String eq 'Rush'"},
            {x => "Rush" == x.String, "'Rush' eq String"},
            {x => x.String != "Rush", "String ne 'Rush'"},
            {x => "Rush" != x.String, "'Rush' ne String"},

            {x => x.DateTime == RushTime, "DateTime eq datetime'2112-12-21T00:00:00Z'"},
            {x => RushTime == x.DateTime, "datetime'2112-12-21T00:00:00Z' eq DateTime"},
            {x => x.DateTime != RushTime, "DateTime ne datetime'2112-12-21T00:00:00Z'"},
            {x => RushTime != x.DateTime, "datetime'2112-12-21T00:00:00Z' ne DateTime"},
            {x => x.DateTime > RushTime, "DateTime gt datetime'2112-12-21T00:00:00Z'"},
            {x => RushTime > x.DateTime, "datetime'2112-12-21T00:00:00Z' gt DateTime"},
            {x => x.DateTime < RushTime, "DateTime lt datetime'2112-12-21T00:00:00Z'"},
            {x => RushTime < x.DateTime, "datetime'2112-12-21T00:00:00Z' lt DateTime"},

            {x => x.Guid == RushId, $"Guid eq guid'{RushId}'"},
            {x => RushId == x.Guid, $"guid'{RushId}' eq Guid"},
            {x => x.Guid != RushId, $"Guid ne guid'{RushId}'"},
            {x => RushId != x.Guid, $"guid'{RushId}' ne Guid"},

            // {x => x.Enum == TestEnum.Hemispheres, "Enum eq 2"},
            // {x => TestEnum.AFarewellToKings == x.Enum, "1 eq Enum"},
            // {x => x.Enum != TestEnum.AFarewellToKings, "Enum ne 1"},
            // {x => TestEnum.Hemispheres != x.Enum, "2 ne Enum"},

            {x => (x.String == "Rush" || x.Integer == 2112 && (x.Boolean || false)) && x.DateTime == RushTime, "((String eq 'Rush' or (Integer eq 2112 and (Boolean eq true or false))) and DateTime eq datetime'2112-12-21T00:00:00Z')" },
            { GetExpression(x => false).And(x => x.Boolean), "(false and Boolean eq true)" }
        };

        private static Expression<Func<TestEntity, bool>> GetExpression(Expression<Func<TestEntity, bool>> expression)
        {
            return expression;
        }

        [Theory, MemberData(nameof(TestCases))]
        public void Should_generate_query_filter(Expression<Func<TestEntity, bool>> expression, string filter)
        {
            expression.GetTableStorageFilter().ShouldBe(filter);
        }

        [Fact]
        public void Should_work_with_local_variable()
        {
            var localString = "Rush";

            GetExpression(x => x.String == localString).GetTableStorageFilter().ShouldBe("String eq 'Rush'");
        }

        [Fact]
        public void Should_work_with_field()
        {
            GetExpression(x => x.DateTime == RushTime).GetTableStorageFilter().ShouldBe("DateTime eq datetime'2112-12-21T00:00:00Z'");
        }

        [Fact]
        public void Should_work_property()
        {
            var entity = new TestEntity { Integer = 2112 };

            GetExpression(x => x.Integer == entity.Integer).GetTableStorageFilter().ShouldBe("Integer eq 2112");
        }

        public class TestEntity
        {
            public bool Boolean { get; set; }
            public int Integer { get; set; }
            public int? Nullable { get; set; }
            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public Guid Guid { get; set; }
            public TestEnum Enum { get; set; }
            public TestEnum? NullableEnum { get; set; }
        }

        public enum TestEnum
        {
            AFarewellToKings = 1,
            Hemispheres = 2,
            PermanentWaves = 3
        }
    }
}
