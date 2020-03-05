using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Identity.Api.V1;
using Laso.AdminPortal.Web.Configuration;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laso.AdminPortal.Web.Api.Partners
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly IOptionsMonitor<IdentityServiceOptions> _options;
        private readonly ILogger<PartnersController> _logger;
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public PartnersController(
            IOptionsMonitor<IdentityServiceOptions> options,
            ILogger<PartnersController> logger,
            Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _options = options;
            _logger = logger;
            _partnersClient = partnersClient;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] PartnerViewModel partner)
        {
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");
            // var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
            // using var channel = GrpcChannel.ForAddress(_options.CurrentValue.ServiceUrl, new GrpcChannelOptions
            // {
            //     HttpClient = new HttpClient(handler)
            // });
            // var client = new Identity.Api.V1.Partners.PartnersClient(channel);
            try
            {
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
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized();
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
            {
                return Forbid();
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
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");
            // var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
            // using var channel = GrpcChannel.ForAddress(_options.CurrentValue.ServiceUrl, new GrpcChannelOptions
            // {
            //     HttpClient = new HttpClient(handler)
            // });
            // var client = new Identity.Api.V1.Partners.PartnersClient(channel);

            GetPartnersReply reply;
            try
            {
                // reply = await client.GetPartnersAsync(new GetPartnersRequest());
                reply = await _partnersClient.GetPartnersAsync(new GetPartnersRequest());
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized();
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
            {
                return Forbid();
            }

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
            _logger.LogInformation($"Making gRPC call to: {_options.CurrentValue.ServiceUrl}");
            try
            {
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
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized();
            }
            catch (RpcException e) when (e.Status.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
            {
                return Forbid();
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