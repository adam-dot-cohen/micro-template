using System;
using System.IO;
using System.Linq;

namespace Partner.Core.Matching
{
    public static class Normalize
    {
        public static bool NoPunctuation(char input)
        {
            return !char.IsPunctuation(input);
        }

        public static bool PathCharactersOnly(char input)
        {
            return !Path.GetInvalidPathChars().Contains(input);
        }

        public static bool NoPunctuationOrWhitespace(char input)
        {
            return !char.IsPunctuation(input) && !char.IsWhiteSpace(input);
        }

        public static bool DigitsOnly(char input)
        {
            return char.IsDigit(input);
        }

        public static bool LettersOnly(char input)
        {
            return char.IsLetter(input);
        }
    }
}
