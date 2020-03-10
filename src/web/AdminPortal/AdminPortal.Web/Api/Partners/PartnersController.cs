using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Api.V1;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Web.Api.Filters;
using Laso.AdminPortal.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laso.AdminPortal.Web.Api.Partners
{
    // TODO: Could we do this without attributes? [jay_mclain]
    [ApiController]
    [Authorize]
    [HandleRpcExceptions]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly IOptionsMonitor<IdentityServiceOptions> _options;
        private readonly ILogger<PartnersController> _logger;
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;
        private readonly IMediator _mediator;

        public PartnersController(
            IOptionsMonitor<IdentityServiceOptions> options,
            ILogger<PartnersController> logger,
            Identity.Api.V1.Partners.PartnersClient partnersClient,
            IMediator mediator)
        {
            _options = options;
            _logger = logger;
            _partnersClient = partnersClient;
            _mediator = mediator;
        }

        // TODO: Move to command, simplify error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] PartnerViewModel partner)
        {
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");
            var reply = await _partnersClient.CreatePartnerAsync(
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

            return CreatedAtAction(nameof(Get), new { id = partner.Id }, partner);
        }

        // TODO: Move to query, simplify error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");
            var reply = await _partnersClient.GetPartnersAsync(new GetPartnersRequest());

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

        // TODO: Move to query, simplify error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");

            var reply = await _partnersClient.GetPartnerAsync(new GetPartnerRequest { Id = id });

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

        // TODO: Move to query, simplify error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}/configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfiguration([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Query(
                new GetPartnerConfigurationViewModelQuery { PartnerId = id }, cancellationToken);

            return Ok(response.Result);
        }
    }
}