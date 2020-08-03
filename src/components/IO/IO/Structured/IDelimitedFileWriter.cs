using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Laso.IO.Structured
{
    public interface IDelimitedFileWriter
    {
        DelimitedFileConfiguration Configuration { get; set; }

        void Open(Stream stream, Encoding encoding = null);

        void WriteRecords<T>(IEnumerable<T> record);
    }
}
