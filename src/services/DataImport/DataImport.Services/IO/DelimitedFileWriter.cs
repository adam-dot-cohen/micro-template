using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using CsvHelper;
using Laso.DataImport.Core.Extensions;
using System.Globalization;
using System.Reflection;
using CsvHelper.Configuration;

namespace Laso.DataImport.Services.IO
{
    public class DelimitedFileWriter : IDelimitedFileWriter, IDisposable
    {
        private static IEnumerable<Type> ClassMappers;
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;

        public DelimitedFileConfiguration Configuration { get; set; } = new DelimitedFileConfiguration();

        static DelimitedFileWriter()
        {
            InitClassMappers();
        }

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
            ClassMappers.ForEach(m => _csvWriter.Configuration.RegisterClassMap(m));
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

        private static void InitClassMappers()
        {
            if (ClassMappers != null)
                return;
            
            ClassMappers = Assembly.GetExecutingAssembly()
              .GetTypes()
              .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ClassMap)))
              .ToList();
        }
    }
}
