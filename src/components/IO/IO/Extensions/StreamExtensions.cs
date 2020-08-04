using System.IO;
using System.Text;

namespace Laso.IO.Extensions
{
public static class StreamExtensions
    {
        public static byte[] GetBytes(this Stream stream)
        {
            if (stream == null)
                return new byte[0];

            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            using (memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string GetString(this Stream stream, Encoding encoding = null)
        {
            if (stream == null)
                return null;

            using var reader = new StreamReader(stream, encoding ?? Encoding.Default);
            return reader.ReadToEnd();
        }
    }
}
