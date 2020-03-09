using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Laso.DataImport.Services.IO
{
    public interface IDelimitedFileWriter
    {
        DelimitedFileConfiguration Configuration { get; set; }

        void Open(Stream stream, Encoding encoding = null);

        void Close();

        void WriteRecords<T>(IEnumerable<T> record);
    }

    public class DelimitedFileConfiguration
    {
        public bool HasHeaderRecord { get; set; } = true;
        public string Delimiter { get; set; } = ",";
        public bool IgnoreQuotes { get; set; } = false;
        public int IdColumn { get; set; } = 0;
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
