using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Api.V1;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Web.Api.Filters;
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
        private readonly IdentityServiceOptions _options;
        private readonly ILogger<PartnersController> _logger;
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;
        private readonly IMediator _mediator;

        public PartnersController(
            IOptionsMonitor<IdentityServiceOptions> options,
            ILogger<PartnersController> logger,
            Identity.Api.V1.Partners.PartnersClient partnersClient,
            IMediator mediator)
        {
            _options = options.CurrentValue;
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
            _logger.LogInformation($"Making gRPC call to: {_options.ServiceUrl}");
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

        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Query(new GetAllPartnerViewModelsQuery(), cancellationToken);

            return Ok(response.Result);
        }

        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Query(new GetPartnerViewModelQuery() {PartnerId = id}, cancellationToken);

            if (!response.Success)
            {
                return NotFound();
            }

            return Ok(response.Result);
        }

        // TODO: Error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}/configuration")]
        public async Task<IActionResult> GetConfiguration([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Query(
                new GetPartnerConfigurationViewModelQuery { Id = id }, cancellationToken);

            if (response.IsValid && response.Result == null)
                return NotFound(response.ValidationMessages);

            return Ok(response.Result);
        }

        [HttpGet("{id}/pipelineruns")]
        public async Task<IActionResult> GetPipelineRuns([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Query(new GetPartnerPipelineRunsQuery {PartnerId = id}, cancellationToken);

            if (!response.Success)
            {
                return NotFound(response.ValidationMessages);
            }

            return Ok(response.Result);
        }

        [AllowAnonymous]
        [HttpPost("pipelinestatus")]
        public async Task<IActionResult> PostPipelineStatus([FromBody] DataPipelineStatus status, CancellationToken cancellationToken)
        {
            var response = await _mediator.Command(new AddEventToPipelineRunCommand { Event = status }, cancellationToken);

            if (!response.Success)
            {
                throw new Exception(response.GetAllMessages());
            }

            return Ok();
        }
    }
}