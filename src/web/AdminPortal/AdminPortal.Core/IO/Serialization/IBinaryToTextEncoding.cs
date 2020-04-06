using System.Security.Cryptography;

namespace Laso.AdminPortal.Core.IO.Serialization
{
    public interface IBinaryToTextEncoding
    {
        string Encode(byte[] bytes);
        ICryptoTransform GetEncodingTransform();
        byte[] Decode(string encoding);
        ICryptoTransform GetDecodingTransform();
    }
}