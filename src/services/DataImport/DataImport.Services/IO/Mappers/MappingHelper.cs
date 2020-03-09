using System;
using System.Linq;

namespace Laso.DataImport.Services.IO.Mappers
{
    internal static class MappingHelper
    {
        public static string ToUnderscoreDelimited(string words)
        {
            return string.Concat(words.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
        }
    }
}
