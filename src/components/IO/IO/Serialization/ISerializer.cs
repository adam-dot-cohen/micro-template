using System;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.IO.Serialization
{
    public interface ISerializer
    {
        string Serialize<T>(T instance);
        byte[] SerializeToUtf8Bytes<T>(T instance);
        Task Serialize<T>(StreamStack streamStack, T instance, CancellationToken cancellationToken = default);
        string Serialize(Type type, object instance);
        byte[] SerializeToUtf8Bytes(Type type, object instance);
        Task Serialize(StreamStack streamStack, Type type, object instance, CancellationToken cancellationToken = default);
        T Deserialize<T>(string text);
        T DeserializeFromUtf8Bytes<T>(byte[] bytes);
        Task<T> Deserialize<T>(StreamStack streamStack, CancellationToken cancellationToken = default);
        object Deserialize(Type type, string text);
        object DeserializeFromUtf8Bytes(Type type, byte[] bytes);
        Task<object> Deserialize(StreamStack streamStack, Type type, CancellationToken cancellationToken = default);
    }
}
