using System;
using System.Text;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Serialization
{
    public class EncodingJsonSerializer : IJsonSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBinaryToTextEncoding _encoding;

        public EncodingJsonSerializer(IJsonSerializer jsonSerializer, IBinaryToTextEncoding encoding) : this(jsonSerializer, encoding, new JsonSerializationOptions()) { }
        public EncodingJsonSerializer(IJsonSerializer jsonSerializer, IBinaryToTextEncoding encoding, JsonSerializationOptions options)
        {
            _jsonSerializer = jsonSerializer;
            _encoding = encoding;

            _jsonSerializer.SetOptions(options ?? new JsonSerializationOptions());
        }

        public void SetOptions(JsonSerializationOptions options)
        {
            _jsonSerializer.SetOptions(options);
        }

        public async Task<string> Serialize<T>(T instance)
        {
            var bytes = await _jsonSerializer.SerializeToUtf8Bytes(instance);

            return _encoding.Encode(bytes);
        }

        public async Task<byte[]> SerializeToUtf8Bytes<T>(T instance)
        {
            var bytes = await _jsonSerializer.SerializeToUtf8Bytes(instance);

            var text = _encoding.Encode(bytes);

            return Encoding.UTF8.GetBytes(text);
        }

        public async Task<string> Serialize(Type type, object instance)
        {
            var bytes = await _jsonSerializer.SerializeToUtf8Bytes(type, instance);

            return _encoding.Encode(bytes);
        }

        public async Task<byte[]> SerializeToUtf8Bytes(Type type, object instance)
        {
            var bytes = await _jsonSerializer.SerializeToUtf8Bytes(type, instance);

            var text = _encoding.Encode(bytes);

            return Encoding.UTF8.GetBytes(text);
        }

        public async Task<T> Deserialize<T>(string text)
        {
            var bytes = _encoding.Decode(text);

            return await _jsonSerializer.DeserializeFromUtf8Bytes<T>(bytes);
        }

        public async Task<T> DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            var decodedBytes = _encoding.Decode(text);

            return await _jsonSerializer.DeserializeFromUtf8Bytes<T>(decodedBytes);
        }

        public async Task<object> Deserialize(Type type, string text)
        {
            var bytes = _encoding.Decode(text);

            return await _jsonSerializer.DeserializeFromUtf8Bytes(type, bytes);
        }

        public async Task<object> DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            var decodedBytes = _encoding.Decode(text);

            return await _jsonSerializer.DeserializeFromUtf8Bytes(type, decodedBytes);
        }
    }
}
