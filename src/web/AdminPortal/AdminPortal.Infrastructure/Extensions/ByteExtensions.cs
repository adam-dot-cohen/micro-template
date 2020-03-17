using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Laso.AdminPortal.Core.Extensions;

namespace Laso.AdminPortal.Infrastructure.Extensions
{
    public static class ByteExtensions
    {
        public static IEnumerable<byte> ToBytes(this Guid guid)
        {
            return guid.ToString("N").FromHexToBytes();
        }

        private static IEnumerable<byte> FromHexToBytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16));
        }

        public static string Encode(this IEnumerable<byte> bigEndianBytes, Encoding encoding)
        {
            return bigEndianBytes.Encode(encoding.Map);
        }

        public static string Encode(this IEnumerable<byte> bigEndianBytes, string encoding)
        {
            if (encoding.Length < 2)
                throw new ArgumentException("Encoding must have two or more valid characters", nameof(encoding));

            var dividend = new BigInteger(bigEndianBytes.Reverse().Concat((byte) 0).ToArray());
            var chars = new Stack<char>();

            while (dividend != 0)
            {
                dividend = BigInteger.DivRem(dividend, encoding.Length, out var remainder);
                chars.Push(encoding[(int) remainder]);
            }

            return new string(chars.ToArray());
        }
    }

    public sealed class Encoding
    {
        public static readonly Encoding Base26 = new Encoding("abcdefghijklmnopqrtuvwxyz");
        public static readonly Encoding Base36 = new Encoding("0123456789abcdefghijklmnopqrtuvwxyz");
        public static readonly Encoding Base58 = new Encoding("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"); // bitcoin-style

        private Encoding(string map)
        {
            Map = map;
        }

        public string Map { get; }
    }
}
