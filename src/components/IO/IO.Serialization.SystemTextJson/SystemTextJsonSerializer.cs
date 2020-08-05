using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Laso.IO.Serialization.SystemTextJson.Extensions;

namespace Laso.IO.Serialization.SystemTextJson
{
    [Obsolete]
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly ICollection<JsonConverter> _converters;
        private JsonSerializerOptions _options;

        public SystemTextJsonSerializer()
        {
            SetOptions(new JsonSerializationOptions());
        }

        public SystemTextJsonSerializer(JsonConverter converter, params JsonConverter[] converters) : this()
        {
            _converters = converter.Concat(converters).ToList();
        }

        public string Serialize<T>(T instance)
        {
            return JsonSerializer.Serialize(instance, _options);
        }

        public byte[] SerializeToUtf8Bytes<T>(T instance)
        {
            return JsonSerializer.SerializeToUtf8Bytes(instance, _options);
        }

        public async Task Serialize<T>(StreamStack streamStack, T instance, CancellationToken cancellationToken = default)
        {
            await JsonSerializer.SerializeAsync(streamStack.Stream, instance, _options, cancellationToken);
        }

        public string Serialize(Type type, object instance)
        {
            return JsonSerializer.Serialize(instance, type, _options);
        }

        public byte[] SerializeToUtf8Bytes(Type type, object instance)
        {
            return JsonSerializer.SerializeToUtf8Bytes(instance, type, _options);
        }

        public async Task Serialize(StreamStack streamStack, Type type, object instance, CancellationToken cancellationToken = default)
        {
            await JsonSerializer.SerializeAsync(streamStack.Stream, instance, type, _options, cancellationToken);
        }

        public T Deserialize<T>(string text)
        {
            return JsonSerializer.Deserialize<T>(text, _options);
        }

        public T DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return Deserialize<T>(text);
        }

        public async Task<T> Deserialize<T>(StreamStack streamStack, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(streamStack.Stream, _options, cancellationToken);
        }

        public object Deserialize(Type type, string text)
        {
            return JsonSerializer.Deserialize(text, type, _options);
        }

        public object DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return Deserialize(type, text);
        }

        public async Task<object> Deserialize(StreamStack streamStack, Type type, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync(streamStack.Stream, type, _options, cancellationToken);
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = false,
                IgnoreNullValues = !options.IncludeNulls
            };

            foreach (var converter in _converters)
                _options.Converters.Add(converter);

            switch (options.PropertyNameCasingStyle)
            {
                case CasingStyle.Pascal:
                    _options.PropertyNamingPolicy = null;
                    break;
                case CasingStyle.Camel:
                    _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public JsonSerializerOptions GetOptions()
        {
            return _options;
        }
    }
}