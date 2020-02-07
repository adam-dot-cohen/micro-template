using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Domain.Api.Common;

namespace DataImport.Services.DataImport
{
    public interface IDataImporterFactory
    {
        IDataImporter Create(PartnerIdentifier exportFrom, PartnerIdentifier importTo);
    }

    public class DataImporterFactory : IDataImporterFactory
    {
        private readonly IEnumerable<IDataImporter> _importers;
        public DataImporterFactory(IEnumerable<IDataImporter> importers)
        {
            _importers = importers;
        }

        public IDataImporter Create(PartnerIdentifier exportFrom, PartnerIdentifier importTo)
        {
            var importer = _importers.SingleOrDefault(e => e.ExportFrom == exportFrom && e.ImportTo == importTo);

            if (importer == null)
                throw new NotSupportedException($"No implementation found for exports from {nameof(exportFrom)} to {nameof(importTo)}");

            return importer;
        }
    }
}
