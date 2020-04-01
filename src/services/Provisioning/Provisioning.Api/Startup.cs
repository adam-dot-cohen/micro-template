using System.Threading;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.IntegrationEvents;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.Infrastructure.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Provisioning.Infrastructure.Persistence.Azure;
using Serilog;

namespace Laso.Provisioning.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry();
            }

            services.AddGrpc();

            AzureServiceBusTopicProvider GetTopicProvider()
            {
                return new AzureServiceBusTopicProvider(
                    _configuration.GetConnectionString("EventServiceBus"), 
                    _configuration.GetSection("AzureServiceBus").Get<AzureServiceBusConfiguration>());
            }

            services.AddTransient<IEventPublisher>(x => new AzureServiceBusEventPublisher(GetTopicProvider()));
            services.AddSingleton<ISubscriptionProvisioningService, SubscriptionProvisioningService>();
            services.AddSingleton<IApplicationSecrets, AzureApplicationSecrets>();
            services.AddTransient<IBlobStorageService, AzureBlobStorageService>();
            services.AddSingleton<IDataPipelineStorage, AzureDataLakeDataPipelineStorage>();

            services.AddHostedService(sp => new AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>(
                sp.GetService<ILogger<AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>>>(),
                GetTopicProvider(),
                "Provisioning.Api",
                async @event => await sp.GetService<ISubscriptionProvisioningService>()
                                    .ProvisionPartner(@event.Id, @event.NormalizedName, CancellationToken.None)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            // Add gRPC-Web middleware after routing and before endpoints
            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
