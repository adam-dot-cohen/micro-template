using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Laso.Provisioning.Api.HealthChecks
{
    public class ConfigurationHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        private static readonly IEnumerable<string> RequiredConfigurationValues = new[]
        {
            "Services:Provisioning:ConfigurationSecrets:ServiceUrl",
            "Services:Provisioning:IntegrationEventHub:ConnectionString",
            "Services:Provisioning:IntegrationEventHub:TopicNameFormat",
            "Services:Provisioning:PartnerSecrets:ServiceUrl",
            "Services:Provisioning:PartnerEscrowStorage:ServiceUrl",
            "Services:Provisioning:PartnerColdStorage:ServiceUrl",
            "Services:Provisioning:DataProcessingStorage:ServiceUrl"
        };

        public ConfigurationHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            // TODO: Could check invalid, here, too.
            IReadOnlyDictionary<string, object> configurationMissing = RequiredConfigurationValues
                .Where(v => string.IsNullOrWhiteSpace(_configuration[v]))
                .Select(v => new
                {
                    Reason = "Missing",
                    IsMissing = v
                })
                .ToDictionary(a => a.Reason, a => (object)a.IsMissing);

            return Task.FromResult(configurationMissing.Any()
                ? HealthCheckResult.Unhealthy("Configuration values missing.", data: configurationMissing)
                : HealthCheckResult.Healthy());
        }
    }
}
