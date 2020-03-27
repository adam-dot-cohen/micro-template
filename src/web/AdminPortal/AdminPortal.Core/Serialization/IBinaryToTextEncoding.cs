namespace Laso.AdminPortal.Core.Serialization
{
    public interface IBinaryToTextEncoding
    {
        string Encode(byte[] bytes);
        byte[] Decode(string encoding);
    }
}