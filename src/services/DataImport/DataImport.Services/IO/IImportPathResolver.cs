using System;
using DataImport.Domain.Api;
using DataImport.Services.Partners;

namespace DataImport.Services.IO
{
    // [Ed S] if we decide to generate a manifest instead this can all go away
    public interface IImportPathResolver
    {
        string GetIncomingContainerName(string partnerId);
        string GetOutgoingContainerName(string partnerId);
        string GetName(string partnerId, ImportType type, DateTime effectiveDate);
    }

    // todo(ed): need an API in front of this stuff to grab dynamically, support other partners
    public class LasoImportPathResolver : IImportPathResolver
    {
        private readonly IPartnerService _partnerService;

        public LasoImportPathResolver(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        public string GetIncomingContainerName(string partnerId)
        {
            var partner = _partnerService.Get(partnerId);

            return $"partner-{partner.InternalIdentifier}/Incoming";
        }

        public string GetOutgoingContainerName(string partnerId)
        {
            var partner = _partnerService.Get(partnerId);

            return $"partner-{partner.InternalIdentifier}Outgoing";
        }

        public string GetName(string partnerId, ImportType type, DateTime effectiveDate)
        {
            var partner = _partnerService.Get(partnerId);

            // need to grab import config dynamically or just remove frequency from the file name / generate a manifest elsewhere

            return $"{partner.InternalIdentifier}_{PartnerIdentifier.Laso}_{ImportFrequency.Weekly.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
