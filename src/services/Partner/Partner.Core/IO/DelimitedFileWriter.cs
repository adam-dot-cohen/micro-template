using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using CsvHelper;
using Partner.Core.Extensions;
using System.Globalization;

namespace Partner.Core.IO
{
    public class DelimitedFileWriter : IDelimitedFileWriter, IDisposable
    {
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;

        public DelimitedFileConfiguration Configuration { get; set; } = new DelimitedFileConfiguration();

        public void Open(Stream stream, Encoding encoding = default)
        {
            _streamWriter = new StreamWriter(stream, encoding ?? Encoding.UTF8, Configuration.BufferSize);

            var csvWriterConfiguration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = Configuration.HasHeaderRecord,
                Delimiter = Configuration.Delimiter,
                DetectColumnCountChanges = !Configuration.IgnoreExtraColumns
            };

            if (!Configuration.TypeConverterOptions.IsNullOrEmpty())
            {
                Configuration.TypeConverterOptions.ForEach(o =>
                    csvWriterConfiguration.TypeConverterOptionsCache.GetOptions(o.Type).Formats = new[] { o.Format });
            }

            _csvWriter = new CsvWriter(_streamWriter, csvWriterConfiguration, true);
        }

        public void WriteRecords<T>(IEnumerable<T> records)
        {
            if (_csvWriter == null)
                throw new InvalidOperationException("Writer is not initialized.");

            _csvWriter.WriteRecords(records);
            _streamWriter.Flush();
        }

        public void Close()
        {
            if (_streamWriter == null)
                throw new InvalidOperationException("Writer has not been opened");

            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_csvWriter != null)
                {
                    _csvWriter.Dispose();
                    _csvWriter = null;
                }
            }
        }
    }
}
