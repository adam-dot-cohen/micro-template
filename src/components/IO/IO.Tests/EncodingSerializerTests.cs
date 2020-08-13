using System.IO;
using System.Text;
using System.Threading.Tasks;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Shouldly;
using Xunit;

namespace Laso.IO.Tests
{
    public class EncodingSerializerTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var serializer = new EncodingSerializer(new NewtonsoftSerializer(), new Base64Encoding());

            var text = serializer.Serialize(new Test { Property = "Rush" });

            text.ShouldBe("eyJQcm9wZXJ0eSI6IlJ1c2gifQ==");

            var test = serializer.Deserialize<Test>(text);

            test.Property.ShouldBe("Rush");
        }

        [Fact]
        public async Task Should_serialize_and_deserialize_stream()
        {
            var serializer = new EncodingSerializer(new NewtonsoftSerializer(), new Base64Encoding());

            var output = new MemoryStream();

            using (var streamStack = new StreamStack(output))
            {
                await serializer.Serialize(streamStack, new Test { Property = "Rush" });
            }

            var encoded = output.ToArray();

            Encoding.UTF8.GetString(encoded).ShouldBe("eyJQcm9wZXJ0eSI6IlJ1c2gifQ==");

            using (var streamStack = new StreamStack(new MemoryStream(encoded)))
            {
                var test = await serializer.Deserialize<Test>(streamStack);

                test.Property.ShouldBe("Rush");
            }
        }

        private class Test
        {
            public string Property { get; set; }
        }
    }
}