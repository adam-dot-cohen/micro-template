using System;
using System.Text;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Laso.AdminPortal.Infrastructure.Serialization
{
    public class Utf8JsonSerializer : IJsonSerializer
    {
        private IJsonFormatterResolver _resolver;

        public Utf8JsonSerializer() : this(new JsonSerializationOptions()) { }
        public Utf8JsonSerializer(JsonSerializationOptions options)
        {
            SetOptions(options ?? new JsonSerializationOptions());
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            switch (options.PropertyNameCasingStyle)
            {
                case CasingStyle.Pascal:
                    _resolver = options.IncludeNulls ? StandardResolver.AllowPrivate : StandardResolver.AllowPrivateExcludeNull;
                    break;
                case CasingStyle.Camel:
                    _resolver = options.IncludeNulls ? StandardResolver.AllowPrivateCamelCase : StandardResolver.AllowPrivateExcludeNullCamelCase;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<string> Serialize<T>(T instance)
        {
            var bytes = await SerializeToUtf8Bytes(instance);

            return Encoding.UTF8.GetString(bytes);
        }

        public Task<byte[]> SerializeToUtf8Bytes<T>(T instance)
        {
            return Task.FromResult(JsonSerializer.Serialize(instance, _resolver));
        }

        public async Task<string> Serialize(Type type, object instance)
        {
            var bytes = await SerializeToUtf8Bytes(type, instance);

            return Encoding.UTF8.GetString(bytes);
        }

        public Task<byte[]> SerializeToUtf8Bytes(Type type, object instance)
        {
            return Task.FromResult(JsonSerializer.NonGeneric.Serialize(type, instance, _resolver));
        }

        public async Task<T> Deserialize<T>(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            return await DeserializeFromUtf8Bytes<T>(bytes);
        }

        public Task<T> DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(bytes, _resolver));
        }

        public async Task<object> Deserialize(Type type, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            return await DeserializeFromUtf8Bytes(type, bytes);
        }

        public Task<object> DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            return Task.FromResult(JsonSerializer.NonGeneric.Deserialize(type, bytes, _resolver));
        }
    }
}
