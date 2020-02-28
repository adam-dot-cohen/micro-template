using System.Threading;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Core;
using Laso.Provisioning.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Laso.Provisioning.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            services.AddGrpc();

            services.AddTransient<IEventPublisher>(x => new AzureServiceBusEventPublisher(_configuration.GetConnectionString("EventServiceBus")));
            services.AddSingleton<ISubscriptionProvisioningService, SubscriptionProvisioningService>();
            services.AddSingleton<IKeyVaultService, InMemoryKeyVaultService>();

            services.AddHostedService(sp => new AzureServiceBusEventSubscriptionListener<PartnerCreatedEvent>(
                _configuration.GetConnectionString("EventServiceBus"),
                "Provisioning.Api",
                @event => sp.GetService<ISubscriptionProvisioningService>().ProvisionPartner(@event.Id, @event.NormalizedName, CancellationToken.None)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
