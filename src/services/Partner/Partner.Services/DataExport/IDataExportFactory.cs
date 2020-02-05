using System;
using System.Collections.Generic;
using System.Linq;
using Partner.Domain.Laso.Common;

namespace Partner.Services.DataExport
{
    public interface IDataExporterFactory
    {
        IDataExporter Create(PartnerIdentifier partner);
    }

    public class DataExporterFactory : IDataExporterFactory
    {
        private readonly IEnumerable<IDataExporter> _exporters;
        public DataExporterFactory(IEnumerable<IDataExporter> exporters)
        {
            _exporters = exporters;
        }

        public IDataExporter Create(PartnerIdentifier partner)
        {
            var exporter = _exporters.SingleOrDefault(e => e.Partner == partner);

            if (exporter == null)
                throw new NotSupportedException($"No implementation found for {nameof(partner)}");

            return exporter;
        }
    }
}
