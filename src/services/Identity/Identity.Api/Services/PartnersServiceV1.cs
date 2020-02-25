using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Identity.Api.V1;
using Laso.Identity.Core.Messaging;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Domain.Events;

namespace Laso.Identity.Api.Services
{
    public class PartnersServiceV1 : Partners.PartnersBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IEventPublisher _eventPublisher;

        public PartnersServiceV1(ITableStorageService tableStorageService, IEventPublisher eventPublisher)
        {
            _tableStorageService = tableStorageService;
            _eventPublisher = eventPublisher;
        }

        public override async Task<CreatePartnerReply> CreatePartner(CreatePartnerRequest request, ServerCallContext context)
        {
            var inputPartner = request.Partner;
            if (inputPartner == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Partner is required"));
            }

            var normalizedName = new string((inputPartner.NormalizedName ?? inputPartner.Name).ToLower()
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
                Name = inputPartner.Name,
                ContactName = inputPartner.ContactName,
                ContactPhone = inputPartner.ContactPhone,
                ContactEmail = inputPartner.ContactEmail,
                PublicKey = inputPartner.PublicKey,
                NormalizedName = normalizedName
            };

            await _tableStorageService.InsertAsync(partner);

            await _eventPublisher.Publish(new PartnerCreatedEvent
            {
                Id = inputPartner.Id,
                Name = inputPartner.Name,
                NormalizedName = inputPartner.NormalizedName
            });

            return new CreatePartnerReply { Id = inputPartner.Id };
        }

        public override async Task<GetPartnersReply> GetPartners(GetPartnersRequest request, ServerCallContext context)
        {
            var partners = await _tableStorageService.GetAllAsync<Partner>();
            var views = partners.Select(p => new PartnerView
            {
                Id = p.Id,
                Name = p.Name,
                ContactName = p.ContactName,
                ContactPhone = p.ContactPhone,
                ContactEmail = p.ContactEmail
            });

            var reply = new GetPartnersReply();
            reply.Partners.AddRange(views);

            return reply;
        }
    }
}
