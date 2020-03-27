using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Laso.AdminPortal.Infrastructure.Serialization
{
    public class NewtonsoftSerializer : IJsonSerializer
    {
        private JsonSerializerSettings _settings;

        public NewtonsoftSerializer() : this(new JsonSerializationOptions()) { }
        public NewtonsoftSerializer(JsonSerializationOptions options)
        {
            SetOptions(options ?? new JsonSerializationOptions());
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            _settings = new JsonSerializerSettings
            {
                NullValueHandling = options.IncludeNulls ? NullValueHandling.Include : NullValueHandling.Ignore,
                ContractResolver = new ContractResolver
                {
                    NamingStrategy = options.PropertyNameCasingStyle switch
                    {
                        CasingStyle.Pascal => null,
                        CasingStyle.Camel => new CamelCaseNamingStrategy(),
                        _ => throw new ArgumentOutOfRangeException()
                    }
                }
            };
        }

        public async Task<string> Serialize<T>(T instance)
        {
            return await Serialize(typeof(T), instance);
        }

        public async Task<byte[]> SerializeToUtf8Bytes<T>(T instance)
        {
            return await SerializeToUtf8Bytes(typeof(T), instance);
        }

        public Task<string> Serialize(Type type, object instance)
        {
            return Task.FromResult(JsonConvert.SerializeObject(instance, type, _settings));
        }

        public async Task<byte[]> SerializeToUtf8Bytes(Type type, object instance)
        {
            var text = await Serialize(type, instance);

            return Encoding.UTF8.GetBytes(text);
        }

        public Task<T> Deserialize<T>(string text)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(text, _settings));
        }

        public async Task<T> DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return await Deserialize<T>(text);
        }

        public Task<object> Deserialize(Type type, string text)
        {
            return Task.FromResult(JsonConvert.DeserializeObject(text, type, _settings));
        }

        public async Task<object> DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            return await Deserialize(type, text);
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