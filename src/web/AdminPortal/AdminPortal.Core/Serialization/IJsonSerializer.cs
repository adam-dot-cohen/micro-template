namespace Laso.AdminPortal.Core.Serialization
{
    public interface IJsonSerializer : ISerializer
    {
        void SetOptions(JsonSerializationOptions options);
    }
}