using System;
using System.Threading.Tasks;
using DataImport.Domain.Api;
using DataImport.Services.Partners;

namespace DataImport.Services.IO
{
    // [Ed S] if we decide to generate a manifest instead this can all go away
    public interface IImportPathResolver
    {
        Task<string> GetIncomingContainerNameAsync(string partnerId);
        Task<string> GetOutgoingContainerNameAsync(string partnerId);
        Task<string> GetNameAsync(string partnerId, ImportType type, DateTime effectiveDate);
    }

    // todo(ed): need an API in front of this stuff to grab dynamically, support other partners
    public class LasoImportPathResolver : IImportPathResolver
    {
        private readonly IPartnerService _partnerService;

        public LasoImportPathResolver(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        public async Task<string> GetIncomingContainerNameAsync(string partnerId)
        {
            var partner = await _partnerService.GetAsync(partnerId);

            return $"partner-{partner.InternalIdentifier}/Incoming";
        }

        public async Task<string> GetOutgoingContainerNameAsync(string partnerId)
        {
            var partner = await _partnerService.GetAsync(partnerId);

            return $"partner-{partner.InternalIdentifier}Outgoing";
        }

        public async Task<string> GetNameAsync(string partnerId, ImportType type, DateTime effectiveDate)
        {
            var partner = await _partnerService.GetAsync(partnerId);

            // need to grab import config dynamically or just remove frequency from the file name / generate a manifest elsewhere

            return $"{partner.InternalIdentifier}_{PartnerIdentifier.Laso}_{ImportFrequency.Weekly.ShortName()}_{type}_{effectiveDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }
    }
}
