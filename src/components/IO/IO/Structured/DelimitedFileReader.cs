using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;

namespace Laso.IO.Structured
{
    public class DelimitedFileReader : IDelimitedFileReader, IDisposable
    {
        private CsvReader _csvReader;

        public DelimitedFileConfiguration Configuration { get; set; } = new DelimitedFileConfiguration();

        public void Open(Stream stream)
        {
            var reader = new StreamReader(stream);
            Open(reader);
        }

        public void Open(StreamReader reader)
        {
            _csvReader = new CsvReader(
                reader,
                new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    HasHeaderRecord = Configuration.HasHeaderRecord,
                    MissingFieldFound = !Configuration.IgnoreMissingColumns ? new Action<string[], int, ReadingContext>(ConfigurationFunctions.MissingFieldFound) : null,
                    HeaderValidated = !Configuration.IgnoreMissingColumns ? new Action<bool, string[], int, ReadingContext>(ConfigurationFunctions.HeaderValidated) : null,
                    PrepareHeaderForMatch = (header, index) => Configuration.MatchHeaderCaseSensitive ? header : header.ToLower(),
                    Delimiter = Configuration.Delimiter,
                    IgnoreQuotes = Configuration.IgnoreQuotes,
                    DetectColumnCountChanges = !Configuration.IgnoreExtraColumns,
                    BufferSize = Configuration.BufferSize
                });
        }

        public T ReadRecord<T>()
        {
            if (_csvReader == null)
                throw new InvalidOperationException("Reader is not initialized.");

            return !_csvReader.Read() ? default : _csvReader.GetRecord<T>();
        }

        public T ReadRecord<T>(T anonymousTypeDefinition)
        {
            return ReadRecord<T>();
        }

        public IEnumerable<T> ReadRecords<T>()
        {
            if (_csvReader == null)
                throw new InvalidOperationException("Reader is not initialized.");

            return _csvReader.GetRecords<T>();
        }

        public IEnumerable<T> ReadRecords<T>(T anonymousTypeDefinition)
        {
            return ReadRecords<T>();
        }

        public void Dispose()
        {
            _csvReader?.Dispose();
        }
    }
}
