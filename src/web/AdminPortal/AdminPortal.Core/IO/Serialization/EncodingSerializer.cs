using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.IO.Serialization
{
    public class EncodingSerializer : ISerializer
    {
        private readonly ISerializer _serializer;
        private readonly IBinaryToTextEncoding _encoding;

        public EncodingSerializer(ISerializer serializer, IBinaryToTextEncoding encoding)
        {
            _serializer = serializer;
            _encoding = encoding;
        }

        public string Serialize<T>(T instance)
        {
            var bytes = _serializer.SerializeToUtf8Bytes(instance);

            return _encoding.Encode(bytes);
        }

        public byte[] SerializeToUtf8Bytes<T>(T instance)
        {
            var bytes = _serializer.SerializeToUtf8Bytes(instance);

            var text = _encoding.Encode(bytes);

            return Encoding.UTF8.GetBytes(text);
        }

        public async Task Serialize<T>(StreamStack streamStack, T instance, CancellationToken cancellationToken = default)
        {
            streamStack.Push(new CryptoStream(streamStack.Stream, _encoding.GetEncodingTransform(), CryptoStreamMode.Write));

            await _serializer.Serialize(streamStack, instance, cancellationToken);
        }

        public string Serialize(Type type, object instance)
        {
            var bytes = _serializer.SerializeToUtf8Bytes(type, instance);

            return _encoding.Encode(bytes);
        }

        public byte[] SerializeToUtf8Bytes(Type type, object instance)
        {
            var bytes = _serializer.SerializeToUtf8Bytes(type, instance);

            var text = _encoding.Encode(bytes);

            return Encoding.UTF8.GetBytes(text);
        }

        public async Task Serialize(StreamStack streamStack, Type type, object instance, CancellationToken cancellationToken = default)
        {
            streamStack.Push(new CryptoStream(streamStack.Stream, _encoding.GetEncodingTransform(), CryptoStreamMode.Write));

            await _serializer.Serialize(streamStack, instance, cancellationToken);
        }

        public T Deserialize<T>(string text)
        {
            var bytes = _encoding.Decode(text);

            return _serializer.DeserializeFromUtf8Bytes<T>(bytes);
        }

        public T DeserializeFromUtf8Bytes<T>(byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            var decodedBytes = _encoding.Decode(text);

            return _serializer.DeserializeFromUtf8Bytes<T>(decodedBytes);
        }

        public async Task<T> Deserialize<T>(StreamStack streamStack, CancellationToken cancellationToken = default)
        {
            streamStack.Push(new CryptoStream(streamStack.Stream, _encoding.GetDecodingTransform(), CryptoStreamMode.Read));

            return await _serializer.Deserialize<T>(streamStack, cancellationToken);
        }

        public object Deserialize(Type type, string text)
        {
            var bytes = _encoding.Decode(text);

            return _serializer.DeserializeFromUtf8Bytes(type, bytes);
        }

        public object DeserializeFromUtf8Bytes(Type type, byte[] bytes)
        {
            var text = Encoding.UTF8.GetString(bytes);

            var decodedBytes = _encoding.Decode(text);

            return _serializer.DeserializeFromUtf8Bytes(type, decodedBytes);
        }

        public async Task<object> Deserialize(StreamStack streamStack, Type type, CancellationToken cancellationToken = default)
        {
            streamStack.Push(new CryptoStream(streamStack.Stream, _encoding.GetDecodingTransform(), CryptoStreamMode.Read));

            return await _serializer.Deserialize(streamStack, type, cancellationToken);
        }
    }
}
