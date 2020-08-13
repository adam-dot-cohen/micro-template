using System;
using System.Security.Cryptography;
using System.Text;

namespace Laso.Insights.FunctionalTests.Utils
{
    public class RandomGenerator
    {
        private static Random _rnd = new Random();

        private static string GetRandom(char[] chars, int maxSize)
        {
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }

        public static string GetRandomAlphaNumeric(int maxSize)
        {
            char[] chars = new char[62];
            chars =
                "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            return GetRandom(chars, maxSize);
        }

        public static string GetRandomNumeric(int maxSize)
        {
            char[] chars = new char[62];
            chars =
                "1234567890".ToCharArray();
            return GetRandom(chars, maxSize);
        }

        public static string GetRandomAlpha(int maxSize)
        {
            char[] chars = new char[62];
            chars =
                "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            return GetRandom(chars, maxSize);
        }


        public static long LongRandomGen()
        {
            byte[] buf = new byte[8];
            _rnd.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            long result = (Math.Abs(longRand % (2000000000000000 - 1000000000000000)) + 1000000000000000);

            long random_seed = _rnd.Next(1000, 5000);
            random_seed = random_seed * result + _rnd.Next(1000, 5000);

            return ((random_seed / 655 % 10000000000000001));
        }
    }
}