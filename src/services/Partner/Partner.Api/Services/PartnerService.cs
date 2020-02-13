using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Partner.Api
{
    public class PartnerService : Partner.PartnerBase
    {
        private readonly ILogger<PartnerService> _logger;
        public PartnerService(ILogger<PartnerService> logger)
        {
            _logger = logger;
        }

        public override Task<GetPartnerReply> GetPartner(GetPartnerRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetPartnerReply
            {
                Id = "1",
                Name = "LASO",
                InternalIdentifier = GetPartnerReply.Types.PartnerIdentifier.Laso
            });
        }
    }
}
