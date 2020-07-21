using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Laso.IO.Serialization.Newtonsoft
{
    public class NewtonsoftSerializer : IJsonSerializer
    {
        private JsonSerializerSettings _settings;

        public NewtonsoftSerializer() : this(new JsonSerializationOptions()) { }
        public NewtonsoftSerializer(JsonSerializationOptions options)
        {
            SetOptions(options ?? new JsonSerializationOptions());
        }

        public string Serialize<T>(T instance)
        {
            return Serialize(typeof(T), instance);
        }

        public byte[] SerializeToUtf8Bytes<T>(T instance)
        {
            return SerializeToUtf8Bytes(typeof(T), instance);
        }

        public async Task Serialize<T>(StreamStack streamStack, T instance, CancellationToken cancellationToken = default)
        {
            await Serialize(streamStack, typeof(T), instance, cancellationToken);
        }

        public string Serialize(Type type, object instance)
        {
            return JsonConvert.SerializeObject(instance, type, _settings);
        }

        public byte[] SerializeToUtf8Bytes(Type type, object instance)
        {
            var text = Serialize(type, instance);

            return Encoding.UTF8.GetBytes(text);
        }

        public Task Serialize(StreamStack streamStack, Type type, object instance, CancellationToken cancellationToken = default)
        {
            var streamWriter = new StreamWriter(streamStack.Stream);
            var jsonTextWriter = new JsonTextWriter(streamWriter);

            streamStack.Push(streamWriter, jsonTextWriter);

            var serializer = JsonSerializer.Create(_settings);
            serializer.Serialize(jsonTextWriter, instance, type);

            return Task.CompletedTask;
        }

        public T Deserialize<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, _settings);
        }

        public T DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return Deserialize<T>(text);
        }

        public Task<T> Deserialize<T>(StreamStack streamStack, CancellationToken cancellationToken = default)
        {
            var (serializer, reader) = GetReader(streamStack);

            return Task.FromResult(serializer.Deserialize<T>(reader));
        }

        public object Deserialize(Type type, string text)
        {
            return JsonConvert.DeserializeObject(text, type, _settings);
        }

        public object DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return Deserialize(type, text);
        }

        public Task<object> Deserialize(StreamStack streamStack, Type type, CancellationToken cancellationToken = default)
        {
            var (serializer, reader) = GetReader(streamStack);

            return Task.FromResult(serializer.Deserialize(reader, type));
        }

        private (JsonSerializer, JsonTextReader) GetReader(StreamStack streamStack)
        {
            var streamReader = new StreamReader(streamStack.Stream);
            var jsonTextReader = new JsonTextReader(streamReader);

            streamStack.Push(streamReader, jsonTextReader);

            return (JsonSerializer.Create(_settings), jsonTextReader);
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            _settings = new JsonSerializerSettings
            {
                NullValueHandling = options.IncludeNulls ? NullValueHandling.Include : NullValueHandling.Ignore
            };

            var contractResolver = new ContractResolver();

            switch (options.PropertyNameCasingStyle)
            {
                case CasingStyle.Pascal:
                    contractResolver.NamingStrategy = null;
                    break;
                case CasingStyle.Camel:
                    contractResolver.NamingStrategy = new CamelCaseNamingStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _settings.ContractResolver = contractResolver;
        }

        private class ContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (member.MemberType == MemberTypes.Property && ((PropertyInfo) member).CanWrite)
                    property.Writable = true;

                return property;
            }
        }
    }
}