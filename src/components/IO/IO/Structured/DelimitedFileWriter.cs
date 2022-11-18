using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.TypeConversion;

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

            _csvWriter = new CsvWriter(_streamWriter, csvWriterConfiguration, true);
            foreach (var option in Configuration.TypeConverterOptions)
            {
                if (option.Type == typeof(DateTime))
                {
                    _csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(new TypeConverterOptions { Formats = new[] { option.Format } });
                    _csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(new TypeConverterOptions { Formats = new[] { option.Format } });
                    continue;
                }
            }

            var options = new TypeConverterOptions { Formats = new[] { "MM/dd/yyyy" } };
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
