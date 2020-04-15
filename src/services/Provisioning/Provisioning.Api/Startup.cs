using System;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.IntegrationEvents;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.Infrastructure.IntegrationEvents;
using Laso.Provisioning.Infrastructure.Persistence.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Laso.Provisioning.Api
{
    public class Startup
    {
        private readonly IHostEnvironment _environment;

        public Startup(IHostEnvironment environment)
        {
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

            services.AddTransient<ISubscriptionProvisioningService, SubscriptionProvisioningService>();

            services.AddTransient<IEventPublisher>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"];
                var topicNameFormat = configuration["Services:Provisioning:IntegrationEventHub:TopicNameFormat"];
                return new AzureServiceBusEventPublisher(
                    new AzureServiceBusTopicProvider(connectionString, topicNameFormat));
            });

            services.AddTransient<IApplicationSecrets>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceUri = new Uri(configuration["Services:Provisioning:PartnerSecrets:ServiceUrl"]);
                return new AzureKeyVaultApplicationSecrets(
                    new SecretClient(serviceUri, new DefaultAzureCredential()));
            });

            services.AddTransient<IEscrowBlobStorageService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceUri = new Uri(configuration["Services:Provisioning:PartnerEscrowStorage:ServiceUrl"]);
                return new AzureBlobStorageService(
                    new BlobServiceClient(serviceUri, new DefaultAzureCredential()));
            });

            services.AddTransient<IColdBlobStorageService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceUri = new Uri(configuration["Services:Provisioning:PartnerColdStorage:ServiceUrl"]);
                return new AzureBlobStorageService(
                    new BlobServiceClient(serviceUri, new DefaultAzureCredential()));
            });

            services.AddTransient(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceUri = new Uri(configuration["Services:Provisioning:DataProcessingPipelineStorage:ServiceUrl"]);
                return new AzureDataLakeDataPipelineStorage(
                    new DataLakeServiceClient(serviceUri, new DefaultAzureCredential()));
            });
            services.AddTransient<IDataPipelineStorage>(sp => 
                sp.GetRequiredService<AzureDataLakeDataPipelineStorage>());
            
            services.AddHostedService(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>>>();

                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"];
                var topicNameFormat = configuration["Services:Provisioning:IntegrationEventHub:TopicNameFormat"];

                return new AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>(
                    logger,

                    new AzureServiceBusTopicProvider(connectionString, topicNameFormat),
                    "Provisioning.Api",
                    async @event => await sp.GetService<ISubscriptionProvisioningService>()
                        .ProvisionPartner(@event.Id, @event.NormalizedName, CancellationToken.None));
            });
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
