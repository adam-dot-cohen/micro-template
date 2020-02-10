using System;
using DataImport.Domain.Api;

namespace DataImport.Services.IO
{
    public interface IImportPathResolver
    {
        string GetIncomingContainerName(string partner);
        string GetOutgoingContainerName(string partner);
        string GetName(string exportedFrom, ImportType type, DateTime effectiveDate);
    }

    // todo(ed): once we have persistent storage we can pull info to form paths and names dynamically
    public class LasoImportPathResolver : IImportPathResolver
    {
        public string GetIncomingContainerName(string partner)
        {
            return $"partner-{partner}/Incoming";
        }

        public string GetOutgoingContainerName(string partner)
        {
            return $"partner-{partner}/Outgoing";
        }

        public string GetName(string exportedFrom, ImportType type, DateTime effectiveDate)
        {
            return "";
            //return $"{exportedFrom}_{PartnerIdentifier.Laso}_{ImportFrequency.Weekly.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
