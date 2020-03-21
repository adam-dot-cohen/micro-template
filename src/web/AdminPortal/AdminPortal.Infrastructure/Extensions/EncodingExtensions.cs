using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Laso.AdminPortal.Infrastructure.Extensions
{
    public static class EncodingExtensions
    {
        public static string Encode(this Guid guid, Encoding encoding)
        {
            return guid.ToString("N").Encode(Encoding.Hexadecimal, encoding);
        }

        public static string Encode(this string str, Encoding from, Encoding to)
        {
            var integer = new BigInteger(0);
            var multiplicand = new BigInteger(1);

            for (var i = 0; i < str.Length; ++i)
            {
                var index = from.CaseSensitive
                    ? from.Map.IndexOf(str[str.Length - 1 - i])
                    : from.Map.IndexOf(str[str.Length - 1 - i].ToString(), StringComparison.InvariantCultureIgnoreCase);

                if (index < 0)
                    throw new ArgumentException("String has characters outside of the specified encoding", nameof(str));

                integer += index*multiplicand;
                multiplicand *= from.Map.Length;
            }

            var chars = new Stack<char>();

            while (integer != 0)
            {
                integer = BigInteger.DivRem(integer, to.Map.Length, out var remainder);
                chars.Push(to.Map[(int) remainder]);
            }

            return new string(chars.ToArray());
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public sealed class Encoding
    {
        public static readonly Encoding Binary = new Encoding("01", false);
        public static readonly Encoding Octal = new Encoding("01234567", false);
        public static readonly Encoding Decimal = new Encoding("0123456789", false);
        public static readonly Encoding Hexadecimal = new Encoding("0123456789abcdef", false);
        public static readonly Encoding Base26 = new Encoding("abcdefghijklmnopqrtuvwxyz", false);
        public static readonly Encoding Base36 = new Encoding("0123456789abcdefghijklmnopqrtuvwxyz", false);
        public static readonly Encoding Base58 = new Encoding("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz", true); // bitcoin-style
        public static readonly Encoding Base64 = new Encoding("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", true);

        private Encoding(string map, bool caseSensitive)
        {
            Map = map;
            CaseSensitive = caseSensitive;
        }

        public string Map { get; }
        public bool CaseSensitive { get; }
    }
}
