using System;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Serialization
{
    public interface ISerializer
    {
        Task<string> Serialize<T>(T instance);
        Task<byte[]> SerializeToUtf8Bytes<T>(T instance);
        Task<string> Serialize(Type type, object instance);
        Task<byte[]> SerializeToUtf8Bytes(Type type, object instance);
        Task<T> Deserialize<T>(string text);
        Task<T> DeserializeFromUtf8Bytes<T>(byte[] bytes);
        Task<object> Deserialize(Type type, string text);
        Task<object> DeserializeFromUtf8Bytes(Type type, byte[] bytes);
    }
}
