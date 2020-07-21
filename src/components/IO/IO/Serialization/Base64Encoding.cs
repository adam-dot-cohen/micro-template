using System;
using System.Security.Cryptography;

namespace Laso.IO.Serialization
{
    public class Base64Encoding :IBinaryToTextEncoding
    {
        public string Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public ICryptoTransform GetEncodingTransform()
        {
            return new ToBase64Transform();
        }

        public byte[] Decode(string encoding)
        {
            return Convert.FromBase64String(encoding);
        }

        public ICryptoTransform GetDecodingTransform()
        {
            return new FromBase64Transform();
        }
    }
}