using System.IO;
using System.IO.Compression;
using DataImport.Core.IO;

namespace DataImport.Core.Extensions
{
    public static class CompressionExtensions
    {
        public static byte[] Compress(this byte[] bytes)
        {
            using var input = new MemoryStream(bytes);
            using var output = new MemoryStream();

            using (var gs = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(gs);
            }

            return output.ToArray();
        }

        public static bool IsCompressed(this byte[] bytes)
        {
            return bytes.Length >= 2 && bytes[0] == 31 && bytes[1] == 139;
        }

        public static byte[] Decompress(this byte[] bytes)
        {
            using var input = new MemoryStream(bytes);
            using var output = new MemoryStream();

            using (var gs = new GZipStream(input, CompressionMode.Decompress))
            {
                gs.CopyTo(output);
            }

            return output.ToArray();
        }

        public static void Compress(this StreamStack streamStack)
        {
            var gZipStream = new GZipStream(streamStack.Stream, CompressionMode.Compress);

            streamStack.Push(gZipStream);
        }

        public static void Decompress(this StreamStack streamStack)
        {
            var gZipStream = new GZipStream(streamStack.Stream, CompressionMode.Decompress);

            streamStack.Push(gZipStream);
        }

        public static void ReadSeekBuffer(this StreamStack streamStack, int seekBackBufferSize = 128)
        {
            var readSeekBufferedStream = new ReadSeekableStream(streamStack.Stream, seekBackBufferSize);

            streamStack.Push(readSeekBufferedStream);
        }
    }
}
