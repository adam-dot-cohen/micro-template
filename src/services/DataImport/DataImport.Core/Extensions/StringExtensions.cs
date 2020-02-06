using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataImport.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string target)
        {
            return !String.IsNullOrEmpty(target);
        }

        public static bool IsNotNullOrWhiteSpace(this string target)
        {
            return !String.IsNullOrWhiteSpace(target);
        }

        public static void IfNotNullOrWhiteSpace(this string target, Action<string> lambda)
        {
            if (!String.IsNullOrWhiteSpace(target)) lambda(target);
        }

        public static TResult IfNotNullOrWhiteSpace<TResult>(this string target, Func<string, TResult> lambda)
        {
            return String.IsNullOrWhiteSpace(target) ? default : lambda(target);
        }

        public static TResult IfNotNullOrWhiteSpace<TResult>(this string target, Func<string, TResult> lambda, TResult defaultResult)
        {
            return String.IsNullOrWhiteSpace(target) ? defaultResult : lambda(target);
        }

        public static TResult IfNotNullOrEmpty<TResult>(this string target, Func<string, TResult> lambda)
        {
            return String.IsNullOrEmpty(target) ? default : lambda(target);
        }

        public static void IfNullOrEmpty(this string target, Action lambda)
        {
            if (String.IsNullOrEmpty(target)) lambda();
        }

        public static TResult IfNullOrEmpty<TResult>(this string target, Func<TResult> lambda, TResult defaultResult = default)
        {
            return String.IsNullOrEmpty(target) ? lambda() : defaultResult;
        }

        public static string RegexReplace(this string value, string pattern, string replacement)
        {
            return Regex.Replace(value, pattern, replacement);
        }

        public static string Clean(this string input, Func<char, bool> normalizer)
        {
            return String.IsNullOrEmpty(input) ? String.Empty : input.Where(normalizer).AsString();
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (value.IsNotNullOrEmpty() && value.Length > maxLength)
                value = value.Substring(0, maxLength);

            return value;
        }

        public static string Right(this string value, int length)
        {
            if (value.Length > length)
                value = value.Substring(value.Length - length, length);

            return value;
        }

        public static bool? TryParseNullableBoolean(this string value, bool? defaultValue = default)
        {
            return Boolean.TryParse(value, out var temp) ? temp : defaultValue;
        }
    }
}
