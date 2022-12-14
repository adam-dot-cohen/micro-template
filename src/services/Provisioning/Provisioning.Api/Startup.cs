using System;
using System.Threading;
using System.Threading.Tasks;
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
using Laso.Provisioning.Api.HealthChecks;
using Laso.Provisioning.Api.IntegrationEvents;
using Laso.Provisioning.Api.Messaging.SFTP;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Extensions;
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
using Microsoft.IdentityModel.Logging;
using Provisioning.Domain.Entities;
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
            IdentityModelEventSource.ShowPII = true; //this should really only be for develop
            services.AddAuthorization();

            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
               // services.AddApplicationInsightsTelemetry();
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
                .AddCheck<EnvironmentHealthCheck>(nameof(EnvironmentHealthCheck))
                .AddCheck<ConfigurationHealthCheck>(nameof(ConfigurationHealthCheck));

            services.AddGrpc();

            services.AddTransient<ISerializer, NewtonsoftSerializer>();
            services.AddTransient<IJsonSerializer, NewtonsoftSerializer>();
            services.AddTransient<IMessageBuilder, DefaultMessageBuilder>();
            services.AddTransient<IMessageSender>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var serviceBusQueueProvider = new AzureServiceBusQueueProvider(configuration.GetSection("Services:Provisioning:IntegrationMessageHub").Get<AzureServiceBusMessageConfiguration>());
                return new AzureServiceBusMessageSender(serviceBusQueueProvider, sp.GetRequiredService<IJsonSerializer>());
            });

            services.AddTransient<IEventPublisher>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var topicProvider = new AzureServiceBusTopicProvider(
                    configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>(), 
                    configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"]);
                return new AzureServiceBusEventPublisher(topicProvider, sp.GetRequiredService<IMessageBuilder>());
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
                return new AzureDataLakeDataPipelineStorage(
                    new DataLakeServiceClient(serviceUri, new DefaultAzureCredential()));
            });
            services.AddTransient<IDataPipelineStorage>(sp => 
                sp.GetRequiredService<AzureDataLakeDataPipelineStorage>());

            services.AddTransient<ICommandHandler<CreatePgpKeySetCommand>,CreatePgpKeySetHandler>();
            services.AddHostedService(GetListenerService<CreatePgpKeySetCommand>);

            services.AddTransient<ICommandHandler<DeletePartnerPgpKeysCommand>, DeletePartnerPgpKeysHandler>();
            services.AddHostedService(GetListenerService<DeletePartnerPgpKeysCommand>);

            services.AddTransient<ICommandHandler<CreateFTPCredentialsCommand>,CreateFTPCredentialsHandler>();
            services.AddHostedService(GetListenerService<CreateFTPCredentialsCommand>);

            services
                .AddTransient<ICommandHandler<DeletePartnerFTPCredentialsCommand>, DeletePartnerFTPCredentialsHandler>();
            services.AddHostedService(GetListenerService<DeletePartnerFTPCredentialsCommand>);

            services.AddTransient<ICommandHandler<CreatePartnerEscrowStorageCommand>,CreatePartnerEscrowStorageHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerEscrowStorageCommand>);

            services
                .AddTransient<ICommandHandler<DeletePartnerEscrowStorageCommand>, DeletePartnerEscrowStorageHandler>();
            services.AddHostedService(GetListenerService<DeletePartnerEscrowStorageCommand>);

            services.AddTransient<ICommandHandler<CreatePartnerColdStorageCommand>,CreatePartnerColdStorageHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerColdStorageCommand>);

            services
                .AddTransient<ICommandHandler<CreatePartnerDataProcessingDirCommand>,
                    CreatePartnerDataProcessingDirHandler>();
            services.AddHostedService(GetListenerService<CreatePartnerDataProcessingDirCommand>);

            services.AddTransient<ICommandHandler<DeletePartnerDataProcessingDirCommand>,
                    DeletePartnerDataProcessingDirHandler>();
            services.AddHostedService(GetListenerService<DeletePartnerDataProcessingDirCommand>);

            services.AddTransient<ISubscriptionProvisioningService, SubscriptionProvisioningService>();

            var listenerCollection = new ListenerCollection();

            AddSubscription<PartnerCreatedEventV1>(listenerCollection,
                sp => async (@event, cancellationToken) => await sp.GetService<ISubscriptionProvisioningService>()
                    .ProvisionPartner(@event.Id, @event.NormalizedName, /*TODO: use the token here?*/ CancellationToken.None));

            AddSubscription<RotatedAdminPasswordEvent>(listenerCollection, sp => async (@event, cancellationToken) => await new SftpAdminCredentialWatcher(sp.GetService<ITableStorageService>()).Handle(@event));
            AddSubscription<RotateAdminPasswordFailedEvent>(listenerCollection, sp => async (@event, cancellationToken) => await new SftpAdminCredentialWatcher(sp.GetService<ITableStorageService>()).Handle(@event));

            AddSubscription<PartnerAccountCreatedEvent>(listenerCollection,
                sp => async (@event, cancellationToken) => await new CompleteProvisioningHandler(sp.GetService<IEventPublisher>()).Handle(@event));

            AddSubscription<PartnerAccountCreationFailedEvent>(listenerCollection,
                sp => async (@event, cancellationToken) => await new CompleteProvisioningHandler(sp.GetService<IEventPublisher>()).Handle(@event));

            AddSubscription<FTPCredentialsCreatedEvent>(listenerCollection, 
                sp => async (@event, cancellationToken) => await new CreateFTPAccountOnFTPCredentialsCreatedHandler(sp.GetRequiredService<IMessageSender>(), sp.GetRequiredService<IApplicationSecrets>(),sp.GetRequiredService<ILogger<CreateFTPAccountOnFTPCredentialsCreatedHandler>>()).Handle(@event));

            //persist events for provisioning history
            AddProvisioningPersistence<FTPCredentialsCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerFTPCredentialsDeletedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerAccountCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerAccountCreationFailedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerColdStorageCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerColdStorageDeletedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerDataProcessingDirCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerDataProcessingDirDeletedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerPgpKeySetCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerPgpKeysDeletedEvent>(listenerCollection);
            AddProvisioningPersistence<EscrowPartnerStorageCreatedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerEscrowStorageDeletedEvent>(listenerCollection);
            AddProvisioningPersistence<UpdatedPartnerPasswordEvent>(listenerCollection);
            AddProvisioningPersistence<FailedToUpdatePartnerPasswordEvent>(listenerCollection);
            AddProvisioningPersistence<DeletePartnerAccountFailedEvent>(listenerCollection);
            AddProvisioningPersistence<PartnerAccountDeletedEvent>(listenerCollection);

            services.AddHostedService(sp => listenerCollection.GetHostedService(sp));
        }

        private static void AddSubscription<T>(
            ListenerCollection listenerCollection,
            Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler,
            string subscriptionSuffix = null,
            string sqlFilter = null,
            ISerializer serializer = null)
        {
            listenerCollection.Add(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var listener = new AzureServiceBusSubscriptionEventListener<T>(
                    new AzureServiceBusTopicProvider(
                        configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>(),
                        configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"]),
                    "Provisioning.Api" + (subscriptionSuffix != null ? "-" + subscriptionSuffix : ""),
                    new DefaultListenerMessageHandler<T>(() =>
                    {
                        var scope = sp.CreateScope();

                        return new ListenerMessageHandlerContext<T>(
                            getEventHandler(scope.ServiceProvider),
                            scope);
                    }, serializer ?? sp.GetRequiredService<IJsonSerializer>()),
                    sqlFilter: sqlFilter,
                    logger: sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<T>>>());

                return listener.Open;
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

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
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
                sp.GetRequiredService<IJsonSerializer>());
        }

        private static void AddProvisioningPersistence<T>(ListenerCollection listenerCollection) where T : ProvisioningActionEvent, IIntegrationEvent
        {
            AddSubscription<T>(listenerCollection, sp => async (@event,cancellationToken) =>
            {
                await sp.GetService<ITableStorageService>().InsertOrReplaceAsync(@event.CloneToPAE());
            },"persistence");
        }
    }
}
