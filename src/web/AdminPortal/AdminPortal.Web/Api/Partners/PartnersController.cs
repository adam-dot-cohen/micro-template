using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.AdminPortal.Core.DataRouter.Queries;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.SignalR;
using Laso.AdminPortal.Web.Api.Filters;
using Laso.Identity.Api.V1;
using Laso.Provisioning.Api.V1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly Provisioning.Api.V1.Partners.PartnersClient _provisioningClient;
        private readonly IMediator _mediator;

        public PartnersController(
            IOptionsMonitor<IdentityServiceOptions> options,
            ILogger<PartnersController> logger,
            Identity.Api.V1.Partners.PartnersClient partnersClient,
            IMediator mediator, Provisioning.Api.V1.Partners.PartnersClient provisioningClient)
        {
            _options = options.CurrentValue;
            _logger = logger;
            _partnersClient = partnersClient;
            _mediator = mediator;
            _provisioningClient = provisioningClient;
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
            var response = await _mediator.Send(new GetAllPartnerViewModelsQuery(), cancellationToken);

            return Ok(response.Result);
        }

        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetPartnerViewModelQuery() {PartnerId = id}, cancellationToken);

            if (!response.Success)
            {
                return NotFound();
            }

            return Ok(response.Result);
        }

        // TODO: Error handing. [jay_mclain]
        // TODO: Could we do this without attributes? [jay_mclain]
        [HttpGet("{id}/configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfiguration([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetPartnerConfigurationViewModelQuery { Id = id }, cancellationToken);

            if (response.Success && response.Result == null)
                return NotFound(response.ValidationMessages);

            return Ok(response.Result);
        }

        [HttpGet("{id}/analysishistory")]
        public async Task<IActionResult> GetAnalysisHistory([FromRoute] string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetPartnerAnalysisHistoryQuery { PartnerId = id }, cancellationToken);

            if (!response.Success)
            {
                return NotFound(response.ValidationMessages);
            }

            return Ok(response.Result);
        }

        [HttpGet("{id}/provisioninghistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProvisioningHistory([FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetPartnerProvisioningHistoryViewModelQuery { Id = id},
                cancellationToken);

            if (!response.Success)
            {
                return NotFound(response.ValidationMessages);
            }

            return Ok(response.Result);
        }

        [HttpDelete(template:"{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePartner([FromRoute] string id, CancellationToken cancellationToken)
        {
            //TODO: what could we do about failures from the services??
            var provisioning = await _provisioningClient.RemovePartnerAsync(new RemovePartnerRequest{ PartnerId = id }, cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(provisioning.PartnerId))
                return BadRequest();

            _logger.LogInformation($"Making gRPC call to: {_options.ServiceUrl}");
            await _partnersClient.DeletePartnerAsync(new DeletePartnerRequest{ Id = id }, cancellationToken: cancellationToken);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("pipelinestatus")]
        public async Task<IActionResult> PostPipelineStatus(
            [FromBody] DataPipelineStatus status,
            [FromServices] IHubContext<DataAnalysisHub> hubContext,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new UpdatePipelineRunAddStatusEventCommand { Event = status }, cancellationToken);

            if (!response.Success)
            {
                throw new Exception(response.GetAllMessages());
            }

            await hubContext.Clients.All.SendAsync("Updated", status, cancellationToken);

            return Ok();
        }
    }
}
