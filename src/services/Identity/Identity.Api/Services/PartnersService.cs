using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Identity.Api;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Core.Persistence;

namespace Laso.Identity.Api.Services
{
    public class PartnersService : Partners.PartnersBase
    {
        private readonly ITableStorageService _tableStorageService;

        public PartnersService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public override async Task<CreatePartnerReply> CreatePartner(CreatePartnerRequest request, ServerCallContext context)
        {
            var normalizedName = new string((request.NormalizedName ?? request.Name).ToLower()
                .Where(char.IsLetterOrDigit)
                .SkipWhile(char.IsDigit)
                .ToArray());

            var existingPartner = await _tableStorageService.FindAllAsync<Partner>($"{nameof(Partner.NormalizedName)} eq '{normalizedName}'", 1);

            if (existingPartner.Any())
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, ""), new Metadata { { nameof(Partner.NormalizedName), "A partner with the same normalized name already exists" } });
            }

            var partner = new Partner
            {
                Name = request.Name,
                ContactName = request.ContactName,
                ContactPhone = request.ContactPhone,
                ContactEmail = request.ContactEmail,
                PublicKey = request.PublicKey,
                NormalizedName = normalizedName
            };

            await _tableStorageService.InsertAsync(partner);

            return new CreatePartnerReply { Id = partner.Id };
        }
    }
}
