﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Provisioning.Api.HealthChecks;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.Infrastructure.Persistence.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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

            services.AddHealthChecks()
                .AddCheck<ConfigurationHealthCheck>(typeof(ConfigurationHealthCheck).Name);

            services.AddGrpc();

            services.AddTransient<ISubscriptionProvisioningService, SubscriptionProvisioningService>();
            services.AddTransient<IJsonSerializer, NewtonsoftSerializer>();

            services.AddTransient<IEventPublisher>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var topicProvider = new AzureServiceBusTopicProvider(
                    configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"],
                    configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>());
                return new AzureServiceBusEventPublisher(topicProvider, new NewtonsoftSerializer());
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
                var serviceUri = new Uri(configuration["Services:Provisioning:DataProcessingStorage:ServiceUrl"]);
                return new AzureDataLakeDataPipelineStorage(
                    new DataLakeServiceClient(serviceUri, new DefaultAzureCredential()));
            });
            services.AddTransient<IDataPipelineStorage>(sp => 
                sp.GetRequiredService<AzureDataLakeDataPipelineStorage>());

            var eventListeners = new EventListenerCollection();

            eventListeners.Add(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var listener = new AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>(
                    new AzureServiceBusTopicProvider(
                        configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"],
                        configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>()),
                    "Provisioning.Api",
                    async (@event, cancellationToken) => await sp.GetService<ISubscriptionProvisioningService>()
                        .ProvisionPartner(@event.Id, @event.NormalizedName, CancellationToken.None),
                    sp.GetRequiredService<IJsonSerializer>(),
                    logger: sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<PartnerCreatedEventV1>>>());

                return x => listener.Open(x);
            });

            services.AddHostedService(sp => eventListeners.GetHostedService(sp));
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

                endpoints.MapHealthChecks(
                    "/health",
                    new HealthCheckOptions
                    {
                        AllowCachingResponses = false,
                        ResponseWriter = JsonHealthReportResponseWriter.WriteResponse,
                        ResultStatusCodes =
                        {
                            [HealthStatus.Healthy] = StatusCodes.Status200OK,
                            [HealthStatus.Degraded] = StatusCodes.Status200OK,
                            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                        }
                    });

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        private class EventListenerCollection
        {
            private readonly ICollection<Func<IServiceProvider, Func<CancellationToken, Task>>> _eventListeners = new List<Func<IServiceProvider, Func<CancellationToken, Task>>>();

            public void Add(Func<IServiceProvider, Func<CancellationToken, Task>> eventListener)
            {
                _eventListeners.Add(eventListener);
            }

            public EventListenerHostedService GetHostedService(IServiceProvider serviceProvider)
            {
                return new EventListenerHostedService(_eventListeners.Select(x => x(serviceProvider)).ToList());
            }

            public class EventListenerHostedService : BackgroundService
            {
                private readonly ICollection<Func<CancellationToken, Task>> _eventListeners;

                public EventListenerHostedService(ICollection<Func<CancellationToken, Task>> eventListeners)
                {
                    _eventListeners = eventListeners;
                }

                protected override Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    return Task.WhenAll(_eventListeners.Select(x => x(stoppingToken)));
                }
            }
        }
    }
}
