using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Core.Serialization;
using Laso.AdminPortal.Infrastructure.Serialization;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests.Infrastructure.Serialization
{
    public class JsonSerializerTests
    {
        public static TheoryData<IJsonSerializer> Serializers =>  new TheoryData<IJsonSerializer>()
            .With(x => typeof(NewtonsoftSerializer).Assembly.GetTypes()
                .Where(y => !y.IsAbstract && !y.IsInterface && typeof(IJsonSerializer).IsAssignableFrom(y))
                .ForEach(y => x.Add(ConstructSerializer(y))));

        private static IJsonSerializer ConstructSerializer(Type type)
        {
            try
            {
                return (IJsonSerializer) Activator.CreateInstance(type);
            }
            catch
            {
                throw new Exception("JsonSerializer is missing a parameterless constructor: " + type.Name);
            }
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task Should_serialize_and_deserialize_with_default_options(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions());

            var text = await serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\"}");

            var test = await serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task Should_serialize_and_deserialize_with_nulls_included(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { IncludeNulls = true });

            var text = await serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\",\"NullString\":null}");

            var test = await serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task Should_serialize_and_deserialize_with_camel_case(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { PropertyNameCasingStyle = CasingStyle.Camel });

            var text = await serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"calculatedProperty\":\"Rush2112\"}");

            var test = await serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
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
    }
}
