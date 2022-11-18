using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Infrastructure.Mediation.Configuration.Lamar;
using Laso.TableStorage;
using Laso.TableStorage.Azure;
using Laso.TableStorage.Azure.PropertyColumnMappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: HostingStartup(typeof(Laso.Identity.DependencyResolution.Lamar.DependencyConfiguration))]

namespace Laso.Identity.DependencyResolution.Lamar
{
    public class DependencyConfiguration 
    {
        public void Configure(WebApplicationBuilder builder)
        {
            builder
                .Host.ConfigureServices((ctx, services) =>
                {
                    var registry = new ServiceRegistry();
                    ConfigureContainer(registry, ctx.Configuration);
                    services.AddLamar(registry);
                })
                .UseSerilog()
                // Must configure Serilog again since Lamar configures a LoggingFactory and so does Serilog
                ;
            var build = builder.Build();
        }

        private static void ConfigureContainer(ServiceRegistry _, IConfiguration configuration)
        {
            _.Scan(scan =>
            {
                scan.Assembly("Laso.Identity.Infrastructure");
                scan.Assembly("Laso.Identity.Core");
                scan.WithDefaultConventions();

                scan.AddMediatorHandlers();
            });

            _.AddMediator().WithDefaultMediatorBehaviors();

            _.For<ITableStorageContext>().Use(ctx => new AzureTableStorageContext(
                configuration.GetConnectionString("IdentityTableStorage"),
                "identity",
                Array.Empty<ISaveChangesDecorator>(),
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
                }));
            _.For<ITableStorageService>().Use<AzureTableStorageService>();
            _.For<ISerializer>().Use<NewtonsoftSerializer>();
            _.For<IJsonSerializer>().Use<NewtonsoftSerializer>();
            _.For<IMessageBuilder>().Use<DefaultMessageBuilder>();
            _.For<IEventPublisher>().Use(ctx =>
                new AzureServiceBusEventPublisher(
                    new AzureServiceBusTopicProvider(
                        configuration.GetSection("AzureServiceBus").Get<AzureServiceBusConfiguration>(),
                        configuration.GetConnectionString("EventServiceBus")), ctx.GetRequiredService<IMessageBuilder>()));
        }
    }
}