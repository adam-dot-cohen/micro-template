using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Laso.TableStorage.Azure;
using Laso.TableStorage.Azure.PropertyColumnMappers;
using Laso.TableStorage.Tests.Extensions;
using Shouldly;
using Xunit;

namespace Laso.TableStorage.Tests.PropertyColumnMappers
{
    public class ComponentPropertyColumnMapperTests
    {
        private static ComponentPropertyColumnMapper GetMapper()
        {
            return new ComponentPropertyColumnMapper(new IPropertyColumnMapper[] { new DefaultPropertyColumnMapper() });
        }

        [Fact]
        public void Should_map_component_property()
        {
            GetMapper().CanMap(GetProperty(x => x.Component)).ShouldBeTrue();
        }

        [Fact]
        public void Should_not_map_not_a_component_property()
        {
            GetMapper().CanMap(GetProperty(x => x.NotAComponent)).ShouldBeFalse();
        }

        [Fact]
        public void Should_map_component_properties_to_columns()
        {
            var columns = GetMapper()
                .MapToColumns(GetProperty(x => x.Component), new Component
                {
                    ComponentProperty1 = "A Farewell to Kings",
                    ComponentProperty2 = 2112
                });

            columns.ShouldContain(x => x.Key == $"{nameof(TestEntity.Component)}_{nameof(Component.ComponentProperty1)}" && (string) x.Value == "A Farewell to Kings");
            columns.ShouldContain(x => x.Key == $"{nameof(TestEntity.Component)}_{nameof(Component.ComponentProperty2)}" && (int) x.Value == 2112);
        }

        [Fact]
        public void Should_map_to_collection_property()
        {
            var propertyValue = (Component) GetMapper()
                .MapToProperty(GetProperty(x => x.Component), new Dictionary<string, object>
                {
                    { $"{nameof(TestEntity.Component)}_{nameof(Component.ComponentProperty1)}", "A Farewell to Kings" },
                    { $"{nameof(TestEntity.Component)}_{nameof(Component.ComponentProperty2)}", 2112 }
                });

            propertyValue.ComponentProperty1.ShouldBe("A Farewell to Kings");
            propertyValue.ComponentProperty2.ShouldBe(2112);
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<TestEntity, T>> expression)
        {
            return expression.GetProperty();
        }

        private class TestEntity
        {
            [Component]
            public Component Component { get; set; }
            public Component NotAComponent { get; set; }
        }
        
        private class Component
        {
            public string ComponentProperty1 { get; set; }
            public int ComponentProperty2 { get; set; }
        }
    }
}
