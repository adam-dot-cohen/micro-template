using System;
using System.Net.Http.Headers;
using Insights.Data.Triggers.App_Start;
using Insights.Data.Triggers.Configuration;
using Insights.Data.Triggers.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Insights.Data.Triggers.App_Start
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITriggerConfig, TriggerConfig>();
            builder.Services.AddTransient<IDatabricksWorkspaceAuthentication, DatabricksWorkspaceAuthentication>();
            builder.Services.AddHttpClient<IDataBricksJobService, DataBricksJobService>((provider, client) =>
            {
                var config = provider.GetRequiredService<ITriggerConfig>();
                var auth = provider.GetRequiredService<IDatabricksWorkspaceAuthentication>();
                var managementToken = auth.GetManagementToken();
                var databricksToken = auth.GetDatabricksTokenAsync();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", databricksToken);
                client.DefaultRequestHeaders.Add("X-Databricks-Azure-SP-Management-Token", managementToken);
                client.DefaultRequestHeaders.Add("X-Databricks-Azure-Workspace-Resource-Id", config.DatabricksWorkspaceResourceId);
                client.BaseAddress = new Uri(config.DatabricksBaseUri);
            });
        }
    }
}