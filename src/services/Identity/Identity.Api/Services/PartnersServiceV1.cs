using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using IdentityServer4.AccessTokenValidation;
using Laso.Identity.Api.Extensions;
using Laso.Identity.Api.V1;
using Laso.Identity.Core.Partners.Commands;
using Laso.Identity.Domain.Entities;
using Laso.TableStorage;
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

        public override async Task<DeletePartnerReply> DeletePartner(DeletePartnerRequest request, ServerCallContext context)
        {
            var response = await _mediator.Send(new DeletePartnerCommand {PartnerId = request.Id});
            response.ThrowRpcIfFailed();

            return new DeletePartnerReply();
        }

        public override async Task<GetPartnerReply> GetPartner(GetPartnerRequest request, ServerCallContext context)
        {
            var p = await _tableStorageService.GetAsync<Partner>(request.Id);
            var view = new PartnerView
            {
                Id = p.Id,
                Name = p.Name,
                ContactName = p.ContactName,
                ContactPhone = p.ContactPhone,
                ContactEmail = p.ContactEmail
            };

            if (view == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Partner not found"), new Metadata { { nameof(Partner.Id), "Partner not found" } });
            }

            var reply = new GetPartnerReply { Partner = view };

            return reply;
        }

        public override async Task<GetPartnersReply> GetPartners(GetPartnersRequest request, ServerCallContext context)
        {
            var ps = await _tableStorageService.GetAllAsync<Partner>(x=>true);
                var views = ps.Select(p => new PartnerView
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
