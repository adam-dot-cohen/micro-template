using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Shouldly;
using Xunit;

namespace Laso.Identity.UnitTests.Persistence.Azure.PropertyColumnMappers
{
    public class DelimitedPropertyColumnMapperTests
    {
        [Fact]
        public void Should_map_delimited_collection_property()
        {
            new DelimitedPropertyColumnMapper().CanMap(GetProperty(x => x.Collection)).ShouldBeTrue();
        }

        [Fact]
        public void Should_map_delimited_dictionary_property()
        {
            new DelimitedPropertyColumnMapper().CanMap(GetProperty(x => x.Dictionary)).ShouldBeTrue();
        }

        [Fact]
        public void Should_not_map_non_delimited_property()
        {
            new DelimitedPropertyColumnMapper().CanMap(GetProperty(x => x.NonDelimited)).ShouldBeFalse();
        }

        [Fact]
        public void Should_map_to_collection_column()
        {
            var column = new DelimitedPropertyColumnMapper().MapToColumns(GetProperty(x => x.Collection), new[] { "Hemispheres", "A Farewell to Kings" }).Single();

            column.Key.ShouldBe(nameof(TestEntity.Collection));
            column.Value.ShouldBe("Hemispheres|A Farewell to Kings");
        }

        [Fact]
        public void Should_map_to_dictionary_column()
        {
            var column = new DelimitedPropertyColumnMapper().MapToColumns(GetProperty(x => x.Dictionary), new Dictionary<string, string>
            {
                { "Geddy Lee", "Bass/Keyboards/Vocals" },
                { "Neil Peart", "Drums" },
                { "Alex Lifeson", "Guitar" },
            }).Single();

            column.Key.ShouldBe(nameof(TestEntity.Dictionary));
            column.Value.ShouldBe("Geddy Lee;Bass/Keyboards/Vocals|Neil Peart;Drums|Alex Lifeson;Guitar");
        }

        [Fact]
        public void Should_map_to_collection_property()
        {
            var propertyValue = (ICollection<string>) new DelimitedPropertyColumnMapper().MapToProperty(GetProperty(x => x.Collection), new Dictionary<string, object> { { nameof(TestEntity.Collection), "Hemispheres|A Farewell to Kings" } });

            propertyValue.ShouldContain("Hemispheres");
            propertyValue.ShouldContain("A Farewell to Kings");
        }

        [Fact]
        public void Should_map_to_dictionary_property()
        {
            var propertyValue = (IDictionary<string, string>) new DelimitedPropertyColumnMapper().MapToProperty(GetProperty(x => x.Dictionary), new Dictionary<string, object> { { nameof(TestEntity.Dictionary), "Geddy Lee;Bass/Keyboards/Vocals|Neil Peart;Drums|Alex Lifeson;Guitar" } });

            propertyValue.ShouldContain(x => x.Key == "Geddy Lee" && x.Value == "Bass/Keyboards/Vocals");
            propertyValue.ShouldContain(x => x.Key == "Neil Peart" && x.Value == "Drums");
            propertyValue.ShouldContain(x => x.Key == "Alex Lifeson" && x.Value == "Guitar");
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<TestEntity, T>> expression)
        {
            return expression.GetProperty();
        }

        private class TestEntity
        {
            [Delimited]
            public ICollection<string> Collection { get; set; }
            [Delimited]
            public IDictionary<string, string> Dictionary { get; set; }
            public ICollection<string> NonDelimited { get; set; }
        }
    }
}
