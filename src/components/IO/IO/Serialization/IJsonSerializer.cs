namespace Laso.IO.Serialization
{
    public interface IJsonSerializer : ISerializer
    {
        void SetOptions(JsonSerializationOptions options);
    }
}