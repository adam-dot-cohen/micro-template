using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Persistence.Azure.PropertyColumnMappers
{
    public class EnumPropertyColumnMapperTests
    {
        [Fact]
        public void Should_map_enum_property()
        {
            new EnumPropertyColumnMapper().CanMap(GetProperty(x => x.Enum)).ShouldBeTrue();
        }

        [Fact]
        public void Should_map_nullable_enum_property()
        {
            new EnumPropertyColumnMapper().CanMap(GetProperty(x => x.NullableEnum)).ShouldBeTrue();
        }

        [Fact]
        public void Should_not_map_not_an_enum_property()
        {
            new EnumPropertyColumnMapper().CanMap(GetProperty(x => x.NotAnEnum)).ShouldBeFalse();
        }

        [Fact]
        public void Should_map_to_enum_column()
        {
            var column = new EnumPropertyColumnMapper().MapToColumns(GetProperty(x => x.Enum), TestEnum.Hemispheres).Single();

            column.Key.ShouldBe(nameof(TestEntity.Enum));
            column.Value.ShouldBe((int) TestEnum.Hemispheres);
        }

        [Fact]
        public void Should_map_to_nullable_enum_column()
        {
            var column = new EnumPropertyColumnMapper().MapToColumns(GetProperty(x => x.NullableEnum), TestEnum.Hemispheres).Single();

            column.Key.ShouldBe(nameof(TestEntity.NullableEnum));
            column.Value.ShouldBe((int) TestEnum.Hemispheres);
        }

        [Fact]
        public void Should_map_to_nullable_enum_column_with_null()
        {
            var column = new EnumPropertyColumnMapper().MapToColumns(GetProperty(x => x.NullableEnum), null).Single();

            column.Key.ShouldBe(nameof(TestEntity.NullableEnum));
            column.Value.ShouldBeNull();
        }

        [Fact]
        public void Should_map_to_enum_property()
        {
            var propertyValue = new EnumPropertyColumnMapper().MapToProperty(GetProperty(x => x.Enum), new Dictionary<string, object> { { nameof(TestEntity.Enum), 2 } });

            propertyValue.ShouldBe(TestEnum.Hemispheres);
        }

        [Fact]
        public void Should_map_to_enum_property_with_default_value()
        {
            var propertyValue = new EnumPropertyColumnMapper().MapToProperty(GetProperty(x => x.Enum), new Dictionary<string, object> { { nameof(TestEntity.Enum), null } });

            propertyValue.ShouldBe(0);
        }

        [Fact]
        public void Should_map_to_nullable_enum_property()
        {
            var propertyValue = new EnumPropertyColumnMapper().MapToProperty(GetProperty(x => x.NullableEnum), new Dictionary<string, object> { { nameof(TestEntity.NullableEnum), 2 } });

            propertyValue.ShouldBe(TestEnum.Hemispheres);
        }

        [Fact]
        public void Should_map_to_nullable_enum_property_with_null()
        {
            var propertyValue = new EnumPropertyColumnMapper().MapToProperty(GetProperty(x => x.NullableEnum), new Dictionary<string, object> { { nameof(TestEntity.NullableEnum), null } });

            propertyValue.ShouldBeNull();
        }

        [Fact]
        public void Should_map_enum_query_parameter()
        {
            var queryParameter = new EnumPropertyColumnMapper().MapToQuery(GetProperty(x => x.Enum), TestEnum.Hemispheres);

            queryParameter.ShouldBe("2");
        }

        [Fact]
        public void Should_map_nullable_enum_query_parameter()
        {
            var queryParameter = new EnumPropertyColumnMapper().MapToQuery(GetProperty(x => x.NullableEnum), TestEnum.Hemispheres);

            queryParameter.ShouldBe("2");
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<TestEntity, T>> expression)
        {
            return expression.GetProperty();
        }

        private class TestEntity
        {
            public TestEnum Enum { get; set; }
            public TestEnum? NullableEnum { get; set; }
            public bool NotAnEnum { get; set; }
        }

        private enum TestEnum
        {
            AFarewellToKings = 1,
            Hemispheres = 2,
            PermanentWaves = 3
        }
    }
}
