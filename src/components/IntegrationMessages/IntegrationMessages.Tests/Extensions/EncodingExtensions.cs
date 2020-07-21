using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Laso.IntegrationMessages.Tests.Extensions
{
    internal static class EncodingExtensions
    {
        public static string Encode(this Guid guid, IntegerEncoding encoding)
        {
            return guid.ToString("N").Encode(IntegerEncoding.Hexadecimal, encoding);
        }

        public static string Encode(this int integer, IntegerEncoding encoding)
        {
            return integer.ToString().Encode(IntegerEncoding.Decimal, encoding);
        }

        private static string Encode(this string str, IntegerEncoding from, IntegerEncoding to)
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

                integer += index * multiplicand;
                multiplicand *= from.Map.Length;
            }

            var chars = new Stack<char>();

            while (integer != 0)
            {
                integer = BigInteger.DivRem(integer, to.Map.Length, out var remainder);
                chars.Push(to.Map[(int)remainder]);
            }

            return new string(chars.ToArray());
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal sealed class IntegerEncoding
    {
        public static readonly IntegerEncoding Binary = new IntegerEncoding("01", false);
        public static readonly IntegerEncoding Octal = new IntegerEncoding("01234567", false);
        public static readonly IntegerEncoding Decimal = new IntegerEncoding("0123456789", false);
        public static readonly IntegerEncoding Hexadecimal = new IntegerEncoding("0123456789abcdef", false);
        public static readonly IntegerEncoding Base26 = new IntegerEncoding("abcdefghijklmnopqrtuvwxyz", false);
        public static readonly IntegerEncoding Base36 = new IntegerEncoding("0123456789abcdefghijklmnopqrtuvwxyz", false);
        public static readonly IntegerEncoding Base58 = new IntegerEncoding("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz", true); // bitcoin-style
        public static readonly IntegerEncoding Base64 = new IntegerEncoding("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", true);

        private IntegerEncoding(string map, bool caseSensitive)
        {
            Map = map;
            CaseSensitive = caseSensitive;
        }

        public string Map { get; }
        public bool CaseSensitive { get; }
    }
}
