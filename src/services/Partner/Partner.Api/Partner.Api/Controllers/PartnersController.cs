using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Partner.Api.DTOs;
using System.Linq;

namespace Partner.Api.Controllers
{   
    public static class PartnersController
    {
        private static readonly List<PartnerDto> Partners = new List<PartnerDto>
        {
            new PartnerDto
            {
                Id = "1",
                Name = "LASO",
                InternalIdentifier = PartnerIdentifier.Laso.ToString(),
            },
            new PartnerDto
            {
                Id = "2",
                Name = "Quarterspot",
                InternalIdentifier = PartnerIdentifier.Quarterspot.ToString(),                
            },
            new PartnerDto
            {
                Id = "3",
                Name = "Sterling International",
                InternalIdentifier = PartnerIdentifier.SterlingInternational.ToString(),
            },            
            new PartnerDto
            {
                Id = "4",
                Name = "Sona Bank",
                InternalIdentifier = PartnerIdentifier.SonaBank.ToString(),
            }
        };

        [FunctionName(nameof(GetAll))]
        public static async Task<IActionResult> GetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "all")] HttpRequest req,
            ILogger log)
        {
            return await Task.Run(() => new OkObjectResult(Partners));
        }

        // [Ed S]
        // https://github.com/MicrosoftDocs/azure-docs/issues/11755
        // route binding between e.g. /partners/{id} and /partners/search/ will always go with {id}
        // unless we restrict id to a data type. There is currently no way to invoke /search or /all
        // without doing so, even though the URI matches exactly. Alsom, GUID will fail to bind.
        [FunctionName(nameof(Get))]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{id:int}")] HttpRequest req,
            ILogger log,
            int id)
        {
            return await Task.Run(() =>
            {
                var partner = Partners.SingleOrDefault(p => p.Id == id.ToString());
                return partner != null ? (IActionResult)new OkObjectResult(partner) : new NotFoundResult();
            });
        }

        [FunctionName(nameof(Search))]
        public static async Task<IActionResult> Search(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "search")] HttpRequest req,
          ILogger log)
        {
            var internalId = req.Query["internalId"];            

            return await Task.Run(() =>
            {
                var partner = Partners.SingleOrDefault(p => p.InternalIdentifier == internalId);
                return partner != null ? (IActionResult)new OkObjectResult(partner) : new NotFoundResult();
            });
        }

        [FunctionName(nameof(Post))]
        public static async Task<IActionResult> Post(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "")] HttpRequest req,
           ILogger log)
        {
            var body = await req.ReadAsStringAsync();
            PartnerDto newPartner = null;

            try
            {
                newPartner = JsonConvert.DeserializeObject<PartnerDto>(body);
            }
            catch
            {
                return new BadRequestResult();
            }

            return await Task.Run<IActionResult>(() =>
            {
                if (newPartner.Id != null && Partners.Any(p => p.Id == newPartner.Id))
                    return new ConflictResult();

                newPartner.Id = (Partners.Select(p => int.Parse(p.Id)).Max() + 1).ToString();

                return new CreatedAtRouteResult("Partners", new { newPartner.Id }, value: newPartner);
            });
        }

        [FunctionName(nameof(Put))]
        public static async Task<IActionResult> Put(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = "{id}")] HttpRequest req,
          ILogger log,
          string id)
        {
            var body = await req.ReadAsStringAsync();
            PartnerDto partner = null;

            try
            {
                partner = JsonConvert.DeserializeObject<PartnerDto>(body);
            }
            catch
            {
                return new BadRequestResult();
            }

            return await Task.Run<IActionResult>(() =>
            {
                if (!Partners.Any(p => p.Id == partner.Id))
                    return new NotFoundObjectResult($"Partner {id} does not exist");

                return new OkResult();
            });
        }

        [FunctionName(nameof(Delete))]
        public static async Task<IActionResult> Delete(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{id}")] HttpRequest req,
         ILogger log,
         string id)
        {         
            return await Task.Run<IActionResult>(() =>
            {
                var existingPartner = Partners.Single(p => p.Id == id);
                if (existingPartner == null)
                    return new NotFoundObjectResult($"Partner {id} does not exist");

                Partners.Remove(existingPartner);

                return new OkResult();
            });
        }
    }
}
