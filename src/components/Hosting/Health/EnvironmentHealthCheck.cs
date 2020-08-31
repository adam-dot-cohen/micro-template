using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Laso.Hosting.Health
{
    public class EnvironmentHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var response = new Dictionary<string, object?>
            {
                ["MachineName"] = Environment.MachineName,
                ["Version"] = Assembly.GetEntryAssembly()?.GetName().Version.ToString(4),
                ["ServerTime"] = DateTime.UtcNow.ToString("O")
            };

            return Task.FromResult(HealthCheckResult.Healthy(data: response));
        }
    }
}
