using System;
using System.Collections.Generic;
using System.Linq;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    public interface IDataImporterFactory
    {
        IDataImporter Create(PartnerIdentifierDto partner);
    }

    public class DataImporterFactory : IDataImporterFactory
    {
        private readonly IEnumerable<IDataImporter> _importers;
        public DataImporterFactory(IEnumerable<IDataImporter> importers)
        {
            _importers = importers;
        }

        public IDataImporter Create(PartnerIdentifierDto partner)
        {
            var importer = _importers.SingleOrDefault(e => e.Partner == partner);

            if (importer == null)
                throw new NotSupportedException($"No implementation found for exports from {nameof(partner)}");

            return importer;            
        }
    }
}
