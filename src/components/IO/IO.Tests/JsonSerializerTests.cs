using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.IO.Serialization.SystemTextJson;
using Laso.IO.Serialization.Utf8Json;
using Shouldly;
using Xunit;

namespace Laso.IO.Tests
{
    public class JsonSerializerTests
    {
#pragma warning disable 612
        private static readonly Type[] ExcludedSerializers = {typeof(SystemTextJsonSerializer)};

        public static TheoryData<IJsonSerializer> GetSerializers()
        {
            var theoryData = new TheoryData<IJsonSerializer>();

            var types = typeof(NewtonsoftSerializer).Assembly.GetTypes()
                .Concat(typeof(SystemTextJsonSerializer).Assembly.GetTypes())
                .Concat(typeof(Utf8JsonSerializer).Assembly.GetTypes())
                .Where(y => !y.IsAbstract && !y.IsInterface && typeof(IJsonSerializer).IsAssignableFrom(y))
                .Where(y => !ExcludedSerializers.Contains(y));

            foreach (var type in types)
                theoryData.Add(ConstructSerializer(type));

            return theoryData;
        }
#pragma warning restore 612

        private static IJsonSerializer ConstructSerializer(Type type)
        {
            var constructors = type.GetConstructors();

            var defaultConstructor = constructors.FirstOrDefault(x => x.GetParameters().Length == 0);

            if (defaultConstructor == null)
                throw new Exception("JsonSerializer is missing an default constructor: " + type.Name);

            return (IJsonSerializer) defaultConstructor.Invoke(new object[0]);
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public void Should_serialize_and_deserialize_with_default_options(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions());

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\"}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public void Should_serialize_and_deserialize_with_nulls_included(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { IncludeNulls = true });

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\",\"NullString\":null}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public void Should_serialize_and_deserialize_with_camel_case(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { PropertyNameCasingStyle = CasingStyle.Camel });

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"calculatedProperty\":\"Rush2112\"}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public async Task Should_serialize_and_deserialize_stream(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions());

            var output = new MemoryStream();

            using (var streamStack = new StreamStack(output))
            {
                await serializer.Serialize(streamStack, new Test { Property = "Rush" });
            }

            var encoded = output.ToArray();

            Encoding.UTF8.GetString(encoded).ShouldBe("{\"CalculatedProperty\":\"Rush2112\"}");

            using (var streamStack = new StreamStack(new MemoryStream(encoded)))
            {
                var test = await serializer.Deserialize<Test>(streamStack);

                test.Property.ShouldBe("Rush");
            }
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public void Should_deserialize_class_with_no_default_constructor(IJsonSerializer serializer)
        {
            var text = serializer.Serialize(new TestNonDefaultConstructor("Rush") { Property = "2112" });

            text.ShouldBe("{\"ConstructorProperty\":\"Rush\",\"Property\":\"2112\"}");

            var test = serializer.Deserialize<TestNonDefaultConstructor>(text);

            test.ConstructorProperty.ShouldBe("Rush");
            test.Property.ShouldBe("2112");
        }

        [Theory]
        [MemberData(nameof(GetSerializers))]
        public void Should_deserialize_class_with_readonly_collection(IJsonSerializer serializer)
        {
            var text = serializer.Serialize(new TestReadOnlyCollection { Collection = new[] { "2112", "A Farewell to Kings", "Signals" } });

            text.ShouldBe("{\"Collection\":[\"2112\",\"A Farewell to Kings\",\"Signals\"]}");

            var test = serializer.Deserialize<TestReadOnlyCollection>(text);

            test.Collection.ShouldContain("2112");
            test.Collection.ShouldContain("A Farewell to Kings");
            test.Collection.ShouldContain("Signals");
        }

        private class Test
        {
            public string CalculatedProperty { get; private set; }

            [IgnoreDataMember]
            public string Property
            {
                get => CalculatedProperty?.Replace("2112", "");
                set => CalculatedProperty = value + "2112";
            }

            public string NullString { get; private set; }
        }

        private class TestNonDefaultConstructor
        {
            public string ConstructorProperty { get; }
            public string Property { get; set; }

            public TestNonDefaultConstructor(string constructorProperty)
            {
                ConstructorProperty = constructorProperty;
            }
        }

        private class TestReadOnlyCollection
        {
            public IReadOnlyCollection<string> Collection { get; set; }
        }
    }
}
