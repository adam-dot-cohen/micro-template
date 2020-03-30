using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Core.IO;
using Laso.AdminPortal.Core.IO.Serialization;
using Laso.AdminPortal.Infrastructure.IO.Serialization;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests.IO.Serialization
{
    public class JsonSerializerTests
    {
#pragma warning disable 612
        private static readonly Type[] ExcludedSerializers = {typeof(SystemTextJsonSerializer)};
#pragma warning restore 612

        public static TheoryData<IJsonSerializer> Serializers =>  new TheoryData<IJsonSerializer>()
            .With(x => typeof(NewtonsoftSerializer).Assembly.GetTypes()
                .Where(y => !y.IsAbstract && !y.IsInterface && typeof(IJsonSerializer).IsAssignableFrom(y))
                .Where(y => !ExcludedSerializers.Contains(y))
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
        public void Should_serialize_and_deserialize_with_default_options(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions());

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\"}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public void Should_serialize_and_deserialize_with_nulls_included(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { IncludeNulls = true });

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"CalculatedProperty\":\"Rush2112\",\"NullString\":null}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public void Should_serialize_and_deserialize_with_camel_case(IJsonSerializer serializer)
        {
            serializer.SetOptions(new JsonSerializationOptions { PropertyNameCasingStyle = CasingStyle.Camel });

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("{\"calculatedProperty\":\"Rush2112\"}");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
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
