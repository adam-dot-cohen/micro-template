using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Services.DataImport
{
    public interface IDataImporterFactory
    {
        IDataImporter Create(string partnerId);
    }

    public class DataImporterFactory : IDataImporterFactory
    {
        private readonly IEnumerable<IDataImporter> _importers;
        public DataImporterFactory(IEnumerable<IDataImporter> importers)
        {
            _importers = importers;
        }

        public IDataImporter Create(string partnerId)
        {
            //var importer = _importers.SingleOrDefault(e => e.PartnerId == partnerId);

            //if (importer == null)
            //    throw new NotSupportedException($"No implementation found for exports from {nameof(partnerId)}");

            //return importer;
            return null;
        }
    }
}
