using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Web.Partners
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly ILogger<PartnersController> _logger;

        public PartnersController(ILogger<PartnersController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public PartnerViewModel Post([FromBody] PartnerViewModel partner)
        {
            partner.Id = Guid.NewGuid().ToString("D");
            _logger.LogInformation("Saved partner {@NewPartner}", partner);

            return partner;
        }
    }

    // [Authorize]
    // [ApiController]
    // [Route("api/[controller]")]
    // public class PartnersController : ControllerBase
    // {
    //     [HttpPost]
    //     public async Task<PartnerViewModel> Create(PartnerViewModel partner)
    //     {
    //         using var channel = GrpcChannel.ForAddress("https://localhost:5201");
    //         var client = new Identity.Api.Partners.PartnersClient(channel);
    //         var reply = await client.CreatePartnerAsync(
    //             new CreatePartnerRequest
    //             {
    //                 Name = partner.Name,
    //                 ContactName = partner.ContactName,
    //                 ContactPhone = partner.ContactPhone,
    //                 ContactEmail = partner.ContactEmail,
    //                 PublicKey = partner.PublicKey,
    //                 ResourcePrefix = partner.Name.ToLowerInvariant()
    //             });
    //
    //         partner.Id = reply.Id;
    //
    //         return partner;
    //     }
    // }

    public class PartnerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string PublicKey { get; set; }
    }
}