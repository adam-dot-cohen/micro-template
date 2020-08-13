using System.Collections.Generic;
using System.IO;

namespace Laso.IO.Structured
{
    public interface IDelimitedFileReader
    {
        DelimitedFileConfiguration Configuration { get; set; }

        void Open(Stream stream);

        T ReadRecord<T>();
        T ReadRecord<T>(T anonymousTypeDefinition);
        IEnumerable<T> ReadRecords<T>();
        IEnumerable<T> ReadRecords<T>(T anonymousTypeDefinition);
    }
}
