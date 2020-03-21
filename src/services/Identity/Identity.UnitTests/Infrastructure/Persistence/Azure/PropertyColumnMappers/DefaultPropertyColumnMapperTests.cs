using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Infrastructure.Persistence.Azure.PropertyColumnMappers
{
    public class DefaultPropertyColumnMapperTests
    {
        [Fact]
        public void Should_map_everything()
        {
            new DefaultPropertyColumnMapper().CanMap(null).ShouldBeTrue();
        }

        [Fact]
        public void Should_map_to_column()
        {
            var column = new DefaultPropertyColumnMapper().MapToColumns(GetProperty(x => x.Boolean), true).Single();

            column.Key.ShouldBe(nameof(TestEntity.Boolean));
            column.Value.ShouldBe(true);
        }

        [Fact]
        public void Should_map_to_nullable_column()
        {
            var column = new DefaultPropertyColumnMapper().MapToColumns(GetProperty(x => x.Nullable), 2112).Single();

            column.Key.ShouldBe(nameof(TestEntity.Nullable));
            column.Value.ShouldBe(2112);
        }

        [Fact]
        public void Should_map_to_nullable_column_with_null()
        {
            var column = new DefaultPropertyColumnMapper().MapToColumns(GetProperty(x => x.Nullable), null).Single();

            column.Key.ShouldBe(nameof(TestEntity.Nullable));
            column.Value.ShouldBeNull();
        }

        [Fact]
        public void Should_map_to_property()
        {
            var propertyValue = new DefaultPropertyColumnMapper().MapToProperty(GetProperty(x => x.Boolean), new Dictionary<string, object> { { nameof(TestEntity.Boolean), true } });

            propertyValue.ShouldBe(true);
        }

        [Fact]
        public void Should_map_to_nullable_property()
        {
            var propertyValue = new DefaultPropertyColumnMapper().MapToProperty(GetProperty(x => x.Nullable), new Dictionary<string, object> { { nameof(TestEntity.Nullable), 2112 } });

            propertyValue.ShouldBe(2112);
        }

        [Fact]
        public void Should_map_to_nullable_property_with_null()
        {
            var propertyValue = new DefaultPropertyColumnMapper().MapToProperty(GetProperty(x => x.Nullable), new Dictionary<string, object> { { nameof(TestEntity.Nullable), null } });

            propertyValue.ShouldBeNull();
        }

        [Fact]
        public void Should_map_boolean_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.Boolean), true);

            queryParameter.ShouldBe("true");
        }

        [Fact]
        public void Should_map_string_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.String), "Rush");

            queryParameter.ShouldBe("'Rush'");
        }

        [Fact]
        public void Should_map_date_time_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.DateTime), new DateTime(2112, 12, 21));

            queryParameter.ShouldBe("datetime'2112-12-21T00:00:00Z'");
        }

        [Fact]
        public void Should_map_guid_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.Guid), Guid.Parse("12c5c691-4204-4213-9b4c-b7106470b539"));

            queryParameter.ShouldBe("guid'12c5c691-4204-4213-9b4c-b7106470b539'");
        }

        [Fact]
        public void Should_map_value_type_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.Integer), 2112);

            queryParameter.ShouldBe("2112");
        }

        [Fact]
        public void Should_map_nullable_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.Nullable), 2112);

            queryParameter.ShouldBe("2112");
        }

        [Fact]
        public void Should_map_null_query_parameter()
        {
            var queryParameter = new DefaultPropertyColumnMapper().MapToQueryParameter(new TableStorageFilterDialect(), GetProperty(x => x.Nullable), null);

            queryParameter.ShouldBeNull();
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<TestEntity, T>> expression)
        {
            return expression.GetProperty();
        }

        private class TestEntity
        {
            public bool Boolean { get; set; }
            public int Integer { get; set; }
            public int? Nullable { get; set; }
            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public Guid Guid { get; set; }
        }
    }
}
