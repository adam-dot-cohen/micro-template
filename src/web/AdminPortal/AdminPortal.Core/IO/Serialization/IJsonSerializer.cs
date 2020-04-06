namespace Laso.AdminPortal.Core.IO.Serialization
{
    public interface IJsonSerializer : ISerializer
    {
        void SetOptions(JsonSerializationOptions options);
    }
}