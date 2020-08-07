using System;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using IdentityServer4.AccessTokenValidation;
using IntegrationMessages.AzureServiceBus;
using Laso.Hosting;
using Laso.Hosting.Health;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationMessages;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Provisioning.Api.Configuration;
using Laso.Provisioning.Api.Extensions;
using Laso.Provisioning.Api.HealthChecks;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Api.Messaging.SFTP;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.Infrastructure.AzureResources;
using Laso.Provisioning.Infrastructure.Encryption;
using Laso.Provisioning.Infrastructure.Persistence.Azure;
using Laso.Provisioning.Infrastructure.SFTP;
using Laso.TableStorage;
using Laso.TableStorage.Azure;
using Laso.TableStorage.Azure.PropertyColumnMappers;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddAuthorization();

            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry();
            }

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    var authOptions = _configuration.GetSection(AuthenticationOptions.Section).Get<AuthenticationOptions>();
                    options.Authority = authOptions.AuthorityUrl;
                    options.ApiName = authOptions.ClientId;
                    options.ApiSecret = authOptions.ClientSecret;
                });

            // Disable authentication based on settings
            if (!IsAuthenticationEnabled())
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
            }

            services.AddHealthChecks()
                .AddCheck<ConfigurationHealthCheck>(nameof(ConfigurationHealthCheck));

            services.AddGrpc();

            services.AddTransient<ISerializer, NewtonsoftSerializer>();
            services.AddTransient<IJsonSerializer, NewtonsoftSerializer>();
            services.AddTransient<IMessageBuilder, DefaultMessageBuilder>();
            services.AddTransient<IMessageSender>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceBusQueueProvider = new AzureServiceBusQueueProvider(configuration.GetSection("Services:Provisioning:IntegrationMessageHub").Get<AzureServiceBusMessageConfiguration>());
                return new AzureServiceBusMessageSender(serviceBusQueueProvider, new NewtonsoftSerializer());
            });

            services.AddTransient<IEventPublisher>(sp => new AzureServiceBusEventPublisher(GetTopicProvider(sp), sp.GetRequiredService<IMessageBuilder>()));

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

            services.AddTransient<ITableStorageService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var context = new AzureTableStorageContext(configuration["Services:Provisioning:TableStorage:ConnectionString"],
                    "provisioning",
                    new ISaveChangesDecorator[0],
                    new IPropertyColumnMapper[]
                    {
                        new EnumPropertyColumnMapper(),
                        new DelimitedPropertyColumnMapper(),
                        new ComponentPropertyColumnMapper(new IPropertyColumnMapper[]
                        {
                            new EnumPropertyColumnMapper(),
                            new DelimitedPropertyColumnMapper(),
                            new DefaultPropertyColumnMapper()
                        }),
                        new DefaultPropertyColumnMapper()
                    });
                return new AzureTableStorageService(context);
            });

            services.AddTransient<IResourceLocator, ResourceLocator>();

            services.AddTransient(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceUri = new Uri(configuration["Services:Provisioning:DataProcessingStorage:ServiceUrl"]);
                return new AzureDataLakeDataPipelineStorage(new DataLakeServiceClient(serviceUri, new DefaultAzureCredential()));
            });
            services.AddTransient<IDataPipelineStorage>(sp => sp.GetRequiredService<AzureDataLakeDataPipelineStorage>());

            services.AddTransient<ICommandHandler<CreatePgpKeySetCommand>,CreatePgpKeySetHandler>();
            services.AddHostedService(GetListenerService<CreatePgpKeySetCommand>);

            services.AddTransient<ICommandHandler<CreateFTPCredentialsCommand>,CreateFTPCredentialsHandler>();
            services.AddHostedService(GetListenerService<CreateFTPCredentialsCommand>);

            services.AddTransient<ICommandHandler<CreatePartnerEscrowStorageCommand>,CreatePartnerEscrowStorageHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerEscrowStorageCommand>);

            services.AddTransient<ICommandHandler<CreatePartnerColdStorageCommand>,CreatePartnerColdStorageHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerColdStorageCommand>);

            services.AddTransient<ICommandHandler<CreatePartnerDataProcessingDirCommand>, CreatePartnerDataProcessingDirHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerDataProcessingDirCommand>);

            services.AddTransient<ISubscriptionProvisioningService, SubscriptionProvisioningService>();

            var listenerCollection = new ListenerCollection();

            listenerCollection.AddSubscription<PartnerCreatedEventV1>(GetTopicProvider,
                sp => async (@event, cancellationToken) => await sp.GetService<ISubscriptionProvisioningService>()
                    .ProvisionPartner(@event.Id, @event.NormalizedName, /*TODO: use the token here?*/ CancellationToken.None));

            listenerCollection.AddSubscription<PartnerAccountCreatedEvent>(GetTopicProvider,
                sp => async (@event, cancellationToken) => await new CompleteProvisioningHandler(sp.GetService<IEventPublisher>()).Handle(@event));

            listenerCollection.AddSubscription<PartnerAccountCreationFailedEvent>(GetTopicProvider,
                sp => async (@event, cancellationToken) => await new CompleteProvisioningHandler(sp.GetService<IEventPublisher>()).Handle(@event));

            listenerCollection.AddSubscription<FTPCredentialsCreatedEvent>(GetTopicProvider,
                sp => async (@event, cancellationToken) => await new CreateFTPAccountOnFTPCredentialsCreatedHandler(sp.GetRequiredService<IMessageSender>(), sp.GetRequiredService<IApplicationSecrets>(), sp.GetRequiredService<ILogger<CreateFTPAccountOnFTPCredentialsCreatedHandler>>()).Handle(@event));

            services.AddHostedService(sp => listenerCollection.GetHostedService(sp));
        }

        private static AzureServiceBusTopicProvider GetTopicProvider(IServiceProvider sp)
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            return new AzureServiceBusTopicProvider(
                configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>(),
                configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"]);
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

            if (IsAuthenticationEnabled())
                app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PartnersServiceV1>().EnableGrpcWeb();

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

                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                //});
            });
        }

        private bool IsAuthenticationEnabled()
        {
            return _configuration.GetSection(AuthenticationOptions.Section)
                .Get<AuthenticationOptions>()?.Enabled ?? true;
        }

        private static AzureServiceBusQueueListener<T> GetListenerService<T>(IServiceProvider sp) where T : IIntegrationMessage
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return new AzureServiceBusQueueListener<T>(
                sp.GetRequiredService<ILogger<AzureServiceBusQueueListener<T>>>(),
                sp.GetRequiredService<ICommandHandler<T>>(),
                new AzureServiceBusQueueProvider(configuration.GetSection("Services:Provisioning:IntegrationMessageHub").Get<AzureServiceBusMessageConfiguration>()),
                new NewtonsoftSerializer());
        }
    }
}
