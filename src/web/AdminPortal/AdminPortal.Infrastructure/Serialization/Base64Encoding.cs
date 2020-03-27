using System;
using Laso.AdminPortal.Core.Serialization;

namespace Laso.AdminPortal.Infrastructure.Serialization
{
    public class Base64Encoding :IBinaryToTextEncoding
    {
        public string Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public byte[] Decode(string encoding)
        {
            return Convert.FromBase64String(encoding);
        }
    }
}