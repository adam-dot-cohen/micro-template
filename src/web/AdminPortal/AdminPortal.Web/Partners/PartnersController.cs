using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Identity.Api.V1;
using Laso.AdminPortal.Web.Configuration;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laso.AdminPortal.Web.Partners
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly ILogger<PartnersController> _logger;
        private readonly IOptionsMonitor<IdentityServiceOptions> _options;

        public PartnersController(ILogger<PartnersController> logger, IOptionsMonitor<IdentityServiceOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] PartnerViewModel partner)
        {
            using var channel = GrpcChannel.ForAddress(_options.CurrentValue.ServiceUrl);
            var client = new Identity.Api.V1.Partners.PartnersClient(channel);
            try
            {
                var reply = await client.CreatePartnerAsync(
                    new CreatePartnerRequest
                    {
                        Partner = new PartnerModel
                        {
                            Name = partner.Name,
                            ContactName = partner.ContactName,
                            ContactPhone = partner.ContactPhone,
                            ContactEmail = partner.ContactEmail,
                            PublicKey = partner.PublicKey
                        }
                    });
            
                partner.Id = reply.Id;
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
            {
                var errors = new ModelStateDictionary();
                e.Trailers.ForEach(t => errors.AddModelError(t.Key, t.Value));
                return Conflict(errors);
            }
            
            return CreatedAtAction(nameof(Get), new { id = partner.Id }, partner);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            using var channel = GrpcChannel.ForAddress(_options.CurrentValue.ServiceUrl);
            var client = new Identity.Api.V1.Partners.PartnersClient(channel);

            var reply = await client.GetPartnersAsync(new GetPartnersRequest());

            var model = reply.Partners
                .Select(partner => new PartnerViewModel
                {
                    Id = partner.Id,
                    Name = partner.Name,
                    ContactName = partner.ContactName,
                    ContactPhone = partner.ContactPhone,
                    ContactEmail = partner.ContactEmail
                });

            return Ok(model);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            using var channel = GrpcChannel.ForAddress(_options.CurrentValue.ServiceUrl);
            var client = new Identity.Api.V1.Partners.PartnersClient(channel);
            try
            {
                var reply = await client.GetPartnerAsync(new GetPartnerRequest { Id = id });

                var partner = reply.Partner;
                if (partner == null)
                {
                    return NotFound();
                }

                var model = new PartnerViewModel
                {
                    Id = partner.Id,
                    Name = partner.Name,
                    ContactName = partner.ContactName,
                    ContactPhone = partner.ContactPhone,
                    ContactEmail = partner.ContactEmail
                };

                return Ok(model);
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                var errors = new ModelStateDictionary();
                e.Trailers.ForEach(t => errors.AddModelError(t.Key, t.Value));
                return NotFound(errors);
            }
        }
    }
}