using System;
using System.Collections.Generic;

namespace Laso.IO.Structured
{
    public class DelimitedFileConfiguration
    {
        public bool HasHeaderRecord { get; set; } = true;
        public bool MatchHeaderCaseSensitive { get; set; } = true;
        public string Delimiter { get; set; } = ",";
        public bool IgnoreQuotes { get; set; } = false;
        public int IdColumn { get; set; } = 0;
        public bool IgnoreMissingColumns { get; set; } = false;
        public bool IgnoreExtraColumns { get; set; } = false;
        public int BufferSize { get; set; } = 16384; // 16K (bytes)
        public List<TypeConverterOption> TypeConverterOptions { get; set; } = new List<TypeConverterOption>();
    }

    public class TypeConverterOption
    {
        public Type Type { get; set; }
        public string Format { get; set; }
    }
}
