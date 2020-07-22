using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.TableStorage.Azure.PropertyColumnMappers;
using Laso.TableStorage.Tests.Extensions;
using Shouldly;
using Xunit;

namespace Laso.TableStorage.Tests.PropertyColumnMappers
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
        public void Should_map_delimited_enum_collection_property()
        {
            new DelimitedPropertyColumnMapper().CanMap(GetProperty(x => x.EnumCollection)).ShouldBeTrue();
        }

        [Fact]
        public void Should_map_delimited_enum_dictionary_property()
        {
            new DelimitedPropertyColumnMapper().CanMap(GetProperty(x => x.EnumDictionary)).ShouldBeTrue();
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
        public void Should_map_to_enum_collection_column()
        {
            var column = new DelimitedPropertyColumnMapper().MapToColumns(GetProperty(x => x.EnumCollection), new[] { TestEnum.Hemispheres, TestEnum.AFarewellToKings }).Single();

            column.Key.ShouldBe(nameof(TestEntity.EnumCollection));
            column.Value.ShouldBe("2|1");
        }

        [Fact]
        public void Should_map_to_enum_dictionary_column()
        {
            var column = new DelimitedPropertyColumnMapper().MapToColumns(GetProperty(x => x.EnumDictionary), new Dictionary<TestEnum, int>
            {
                { TestEnum.AFarewellToKings, 1 },
                { TestEnum.Hemispheres, 2 },
                { TestEnum.PermanentWaves, 3 }
            }).Single();

            column.Key.ShouldBe(nameof(TestEntity.EnumDictionary));
            column.Value.ShouldBe("1;1|2;2|3;3");
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

        [Fact]
        public void Should_map_to_enum_collection_property()
        {
            var propertyValue = (ICollection<TestEnum>) new DelimitedPropertyColumnMapper().MapToProperty(GetProperty(x => x.EnumCollection), new Dictionary<string, object> { { nameof(TestEntity.EnumCollection), "2|1" } });

            propertyValue.ShouldContain(TestEnum.Hemispheres);
            propertyValue.ShouldContain(TestEnum.AFarewellToKings);
        }

        [Fact]
        public void Should_map_to_enum_dictionary_property()
        {
            var propertyValue = (IDictionary<TestEnum, int>) new DelimitedPropertyColumnMapper().MapToProperty(GetProperty(x => x.EnumDictionary), new Dictionary<string, object> { { nameof(TestEntity.EnumDictionary), "1;1|2;2|3;3" } });

            propertyValue.ShouldContain(x => x.Key == TestEnum.AFarewellToKings && x.Value == 1);
            propertyValue.ShouldContain(x => x.Key == TestEnum.Hemispheres && x.Value == 2);
            propertyValue.ShouldContain(x => x.Key == TestEnum.PermanentWaves && x.Value == 3);
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
            [Delimited]
            public ICollection<TestEnum> EnumCollection { get; set; }
            [Delimited]
            public IDictionary<TestEnum, int> EnumDictionary { get; set; }
            public ICollection<string> NonDelimited { get; set; }
        }

        private enum TestEnum
        {
            AFarewellToKings = 1,
            Hemispheres = 2,
            PermanentWaves = 3
        }
    }
}
