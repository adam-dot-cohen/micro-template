using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace Laso.IO.Structured
{
    public class DelimitedFileWriter : IDelimitedFileWriter, IDisposable
    {
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;

        public DelimitedFileConfiguration Configuration { get; set; } = new DelimitedFileConfiguration();

        public void Open(Stream stream, Encoding encoding = null)
        {
            _streamWriter = new StreamWriter(stream, encoding ?? Encoding.UTF8, Configuration.BufferSize);

            var csvWriterConfiguration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = Configuration.HasHeaderRecord,
                Delimiter = Configuration.Delimiter,
                DetectColumnCountChanges = !Configuration.IgnoreExtraColumns
            };

            Configuration.TypeConverterOptions?.ForEach(o =>
                csvWriterConfiguration.TypeConverterOptionsCache.GetOptions(o.Type).Formats = new[] { o.Format });

            _csvWriter = new CsvWriter(_streamWriter, csvWriterConfiguration, true);
        }

        public void WriteRecords<T>(IEnumerable<T> records)
        {
            if (_csvWriter == null)
                throw new InvalidOperationException("Writer is not initialized.");

            _csvWriter.WriteRecords(records);
            _streamWriter.Flush();
        }

        public void Dispose()
        {
            _csvWriter?.Dispose();
        }
    }
}
