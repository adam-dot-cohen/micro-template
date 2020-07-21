using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Laso.IO.Serialization.Utf8Json
{
    public class Utf8JsonSerializer : IJsonSerializer
    {
        private IJsonFormatterResolver _resolver;

        public Utf8JsonSerializer() : this(new JsonSerializationOptions()) { }
        public Utf8JsonSerializer(JsonSerializationOptions options)
        {
            SetOptions(options ?? new JsonSerializationOptions());
        }

        public string Serialize<T>(T instance)
        {
            var bytes = SerializeToUtf8Bytes(instance);

            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] SerializeToUtf8Bytes<T>(T instance)
        {
            return JsonSerializer.Serialize(instance, _resolver);
        }

        public async Task Serialize<T>(StreamStack streamStack, T instance, CancellationToken cancellationToken = default)
        {
            await JsonSerializer.SerializeAsync(streamStack.Stream, instance, _resolver);
        }

        public string Serialize(Type type, object instance)
        {
            var bytes = SerializeToUtf8Bytes(type, instance);

            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] SerializeToUtf8Bytes(Type type, object instance)
        {
            return JsonSerializer.NonGeneric.Serialize(type, instance, _resolver);
        }

        public async Task Serialize(StreamStack streamStack, Type type, object instance, CancellationToken cancellationToken = default)
        {
            await JsonSerializer.NonGeneric.SerializeAsync(type, streamStack.Stream, instance, _resolver);
        }

        public T Deserialize<T>(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            return DeserializeFromUtf8Bytes<T>(bytes);
        }

        public T DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, _resolver);
        }

        public async Task<T> Deserialize<T>(StreamStack streamStack, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(streamStack.Stream, _resolver);
        }

        public object Deserialize(Type type, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            return DeserializeFromUtf8Bytes(type, bytes);
        }

        public object DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            return Task.FromResult(JsonSerializer.NonGeneric.Deserialize(type, bytes, _resolver));
        }

        public async Task<object> Deserialize(StreamStack streamStack, Type type, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.NonGeneric.DeserializeAsync(type, streamStack.Stream, _resolver);
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            switch (options.PropertyNameCasingStyle)
            {
                case CasingStyle.Pascal:
                    _resolver = (options.IncludeNulls
                        ? StandardResolver.AllowPrivate
                        : StandardResolver.AllowPrivateExcludeNull);
                    break;
                case CasingStyle.Camel:
                    _resolver = (options.IncludeNulls
                        ? StandardResolver.AllowPrivateCamelCase
                        : StandardResolver.AllowPrivateExcludeNullCamelCase);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
