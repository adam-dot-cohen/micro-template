using System.Threading.Tasks;
using Grpc.Core;
using Identity.Api.V1;
using IdentityServer4.AccessTokenValidation;
using Laso.Identity.Core.Partners.Commands;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Laso.Identity.Api.Services
{
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class PartnersServiceV1 : Partners.PartnersBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IMediator _mediator;

        public PartnersServiceV1(ITableStorageService tableStorageService, IMediator mediator)
        {
            _tableStorageService = tableStorageService;
            _mediator = mediator;
        }

        public override async Task<CreatePartnerReply> CreatePartner(CreatePartnerRequest request, ServerCallContext context)
        {
            var inputPartner = request.Partner;
            var command = new CreatePartnerCommand
            {
                Name = inputPartner.Name,
                ContactName = inputPartner.ContactName,
                ContactPhone = inputPartner.ContactPhone,
                ContactEmail = inputPartner.ContactEmail,
                PublicKey = inputPartner.PublicKey,
                NormalizedName = inputPartner.NormalizedName
            };

            var response = await _mediator.Send(command);
            response.ThrowRpcIfFailed();

            return new CreatePartnerReply { Id = response.Result };
        }

        public override async Task<GetPartnerReply> GetPartner(GetPartnerRequest request, ServerCallContext context)
        {
            var view = await _tableStorageService.GetAsync<Partner, PartnerView>(request.Id, p => new PartnerView
            {
                Id = p.Id,
                Name = p.Name,
                ContactName = p.ContactName,
                ContactPhone = p.ContactPhone,
                ContactEmail = p.ContactEmail
            });

            if (view == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Partner not found"), new Metadata { { nameof(Partner.Id), "Partner not found" } });
            }

            var reply = new GetPartnerReply { Partner = view };

            return reply;
        }

        public override async Task<GetPartnersReply> GetPartners(GetPartnersRequest request, ServerCallContext context)
        {
            var views = await _tableStorageService.GetAllAsync<Partner, PartnerView>(p => new PartnerView
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
