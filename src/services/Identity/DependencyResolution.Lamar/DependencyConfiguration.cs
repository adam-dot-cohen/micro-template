using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Infrastructure.IntegrationEvents;
using Laso.Identity.Infrastructure.Mediator.Pipeline;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

[assembly: HostingStartup(typeof(Laso.Identity.DependencyResolution.Lamar.DependencyConfiguration))]

namespace Laso.Identity.DependencyResolution.Lamar
{
    public class DependencyConfiguration : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder
                .ConfigureServices((ctx, services) =>
                {
                    var registry = new ServiceRegistry();
                    ConfigureContainer(registry, ctx.Configuration);
                    services.AddLamar(registry);
                })
                // Must configure Serilog again since Lamar configures a LoggingFactory and so does Serilog
                .UseSerilog();
        }

        private static void ConfigureContainer(ServiceRegistry _, IConfiguration configuration)
        {
            _.Scan(scan =>
            {
                scan.Assembly("Laso.Identity.Infrastructure");
                scan.Assembly("Laso.Identity.Core");
                scan.WithDefaultConventions();

                // Mediator
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
            });

            ConfigureMediator(_);

            _.For<ITableStorageContext>().Use(ctx => new AzureTableStorageContext(
                configuration.GetConnectionString("IdentityTableStorage"),
                "identity",
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
                }));
            _.For<ITableStorageService>().Use<AzureTableStorageService>();
            _.For<IEventPublisher>().Use(ctx =>
                new AzureServiceBusEventPublisher(
                    new AzureServiceBusTopicProvider(
                        configuration.GetConnectionString("EventServiceBus"),
                        configuration.GetSection("AzureServiceBus").Get<AzureServiceBusConfiguration>())));
        }

        private static ServiceRegistry ConfigureMediator(ServiceRegistry _)
        {
            //Pipeline gets executed in order
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(LoggingPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ExceptionPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ValidationPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));

            _.For<IMediator>().Use<Mediator>().Scoped();
            _.For<ServiceFactory>().Use(ctx => ctx.GetInstance);

            return _;
        }

    }
}