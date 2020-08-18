using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Mediation;

namespace Laso.Scheduling.Core.Experiments.Queries
{
    public class GetPartnerExperimentConfigurationQuery : IQuery<PartnerExperimentConfiguration>
    {
        public GetPartnerExperimentConfigurationQuery(string partnerId)
        {
            PartnerId = partnerId;
        }

        public string PartnerId { get; }
    }

    public class PartnerExperimentConfiguration
    {
        public PartnerExperimentConfiguration(string partnerId)
        {
            PartnerId = partnerId;
        }

        public string PartnerId { get; }
        public bool ExperimentsEnabled { get; set; }
    }

    public class GetPartnerExperimentConfigurationHandler : QueryHandler<GetPartnerExperimentConfigurationQuery, PartnerExperimentConfiguration>
    {
        // TODO: Move to service storage
        private static readonly Dictionary<string, PartnerExperimentConfiguration> PartnerConfigs = new[]
            {
                // QuarterSpot
                new PartnerExperimentConfiguration("6c34c5bb-b083-4e62-a83e-cb0532754809") { ExperimentsEnabled = true },

                // Demo
                new PartnerExperimentConfiguration("3051de06-c80f-49e0-9e0d-d7ee5c7d4566") { ExperimentsEnabled = true },
                new PartnerExperimentConfiguration("b5fb980b-4c94-484e-9ae0-176f2972400b") { ExperimentsEnabled = true }
            }
            .ToDictionary(x => x.PartnerId, StringComparer.InvariantCultureIgnoreCase);

        public override Task<QueryResponse<PartnerExperimentConfiguration>> Handle(GetPartnerExperimentConfigurationQuery request, CancellationToken cancellationToken)
        {
            if (!PartnerConfigs.TryGetValue(request.PartnerId, out var value))
            {
                // Default config
                value = new PartnerExperimentConfiguration(request.PartnerId);
            }

            return Task.FromResult(Succeeded(value));
        }
    }
}