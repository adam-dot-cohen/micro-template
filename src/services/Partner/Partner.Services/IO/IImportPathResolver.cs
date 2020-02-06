using System;
using Partner.Domain.Laso.Common;
using Partner.Services.DataImport;

namespace Partner.Services.IO
{
    public interface IImportPathResolver
    {
        string GetIncomingContainerName(PartnerIdentifier partner);
        string GetOutgoingContainerName(PartnerIdentifier partner);
        string GetName(PartnerIdentifier exportedFrom, ImportType type, DateTime effectiveDate);
    }

    // todo(ed): once we have persistent storage we can pull info to form paths and names dynamically
    public class LasoImportPathResolver : IImportPathResolver
    {
        public string GetIncomingContainerName(PartnerIdentifier partner)
        {
            return $"partner-{partner}/Incoming";
        }

        public string GetOutgoingContainerName(PartnerIdentifier partner)
        {
            return $"partner-{partner}/Outgoing";
        }

        public string GetName(PartnerIdentifier exportedFrom, ImportType type, DateTime effectiveDate)
        {
            return $"{exportedFrom}_{PartnerIdentifier.Laso}_{ImportFrequency.Weekly.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
