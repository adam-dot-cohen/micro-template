using System;
using System.Net.Http.Headers;
using Insights.Data.Triggers.App_Start;
using Insights.Data.Triggers.Configuration;
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
            builder.Services.AddHttpClient<IDataBricksJobService, DataBricksJobService>((provider, client) =>
            {
                var config = provider.GetRequiredService<ITriggerConfig>();
                client.BaseAddress = new Uri(config.DataBricksBaseUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.DataBricksBearerToken);
            });
        }
    }
}