﻿using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Identity.Api.V1;
using IdentityServer4.AccessTokenValidation;
using Laso.Identity.Core.Messaging;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.Identity.Domain.Events;
using Microsoft.AspNetCore.Authorization;

namespace Laso.Identity.Api.Services
{
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
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

            var normalizedName = new string((inputPartner.NormalizedName ?? inputPartner.Name).ToLower()
                .Where(char.IsLetterOrDigit)
                .SkipWhile(char.IsDigit)
                .ToArray());

            var existingPartner = await _tableStorageService.FindAllAsync<Partner>(x => x.NormalizedName == normalizedName, 1);

            if (existingPartner.Any())
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Partner already exists"), new Metadata { { nameof(Partner.NormalizedName), "A partner with the same normalized name already exists" } });
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
                Id = partner.Id,
                Name = partner.Name,
                NormalizedName = partner.NormalizedName
            });

            return new CreatePartnerReply { Id = partner.Id };
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