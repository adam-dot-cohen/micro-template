﻿using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.DataRouter.Persistence;
using Laso.AdminPortal.DependencyResolution.Extensions;
using Laso.AdminPortal.Infrastructure.DataRouter.Commands;
using Laso.AdminPortal.Infrastructure.Secrets;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IntegrationMessages.AzureStorageQueue;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation.Configuration.Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.DependencyResolution
{
    public class DependencyConfiguration
    {
        public void Configure(IHostBuilder builder)
        {
            var registry = new ServiceRegistry();
            Initialize(registry);

            builder
                .UseLamar(registry)
                .ConfigureServices((context, services) =>
                {
                    services.AddIdentityServiceGrpcClient(context.Configuration);
                    services.AddProvisioningServiceGrpcClient(context.Configuration);
                });
        }

        private static void Initialize(ServiceRegistry x)
        {
            x.Scan(scan =>
            {
                scan.Assembly("Laso.AdminPortal.Infrastructure");
                scan.WithDefaultConventions();

                scan.AddMediatorHandlers();
            });

            x.AddMediator().WithDefaultMediatorBehaviors();

            x.For<ISerializer>().Use<NewtonsoftSerializer>();
            x.For<IJsonSerializer>().Use<NewtonsoftSerializer>();
            x.For<IApplicationSecrets>().Use<AzureApplicationSecrets>();
            x.For<IDataQualityPipelineRepository>().Use<InMemoryDataQualityPipelineRepository>().Singleton();
            x.For<IEventPublisher>().Use<AzureServiceBusEventPublisher>();

            // NOTE: YES, storage queues are using the table storage connection string!
            // For now we need to reuse the connection string for table storage. dev-ops is looking to define a strategy for
            // managing secrets by service, so not looking to add new secrets in the meantime
            x.ForConcreteType<AzureStorageQueueProvider>().Configure
                .Ctor<AzureServiceBusConfiguration>().Is(s => s.GetInstance<IConfiguration>().GetSection("AzureStorageQueue").Get<AzureServiceBusConfiguration>())
                .Ctor<string>().Is(s => s.GetInstance<IConfiguration>().GetConnectionString("IdentityTableStorage"));

            x.ForConcreteType<AzureServiceBusTopicProvider>().Configure
                .Ctor<AzureServiceBusConfiguration>().Is(s => s.GetInstance<IConfiguration>().GetSection("AzureServiceBus").Get<AzureServiceBusConfiguration>())
                .Ctor<string>().Is(s => s.GetInstance<IConfiguration>().GetConnectionString("EventServiceBus"));

            x.ForConcreteType<AzureServiceBusEventPublisher>().Configure
                .Ctor<IMessageBuilder>()
                .Is<DefaultMessageBuilder>()
                .Named("NonCloudEventPublisher");

            x.ForConcreteType<AzureServiceBusEventPublisher>().Configure
                .Ctor<IMessageBuilder>()
                .Is(s => new CloudEventMessageBuilder(s.GetInstance<NewtonsoftSerializer>(), new Uri("service://data")));

            x.ForConcreteType<NotifyPartnerFilesReceivedHandler>().Configure
                .Ctor<IEventPublisher>()
                .Is(s => s.GetInstance<IEventPublisher>("NonCloudEventPublisher"));
        }
    }
}
