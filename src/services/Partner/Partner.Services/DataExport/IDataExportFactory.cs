using System;
using Partner.Data.Quarterspot;
using Partner.Domain.Common;

namespace Partner.Services.DataExport
{
    public interface IDataExporterFactory
    {
        IDataExporter Create(PartnerIdentifier partner);
    }

    public class DataExporterFactory : IDataExporterFactory
    {
        public IDataExporter Create(PartnerIdentifier partner)
        {
            // todo: Need a service locator
            return partner switch
            {
                PartnerIdentifier.Quarterspot => new QsRepositoryDataExporter(new QuarterspotRepository()),
                _ => throw new NotSupportedException(nameof(partner))
            };
        }
    }
}
