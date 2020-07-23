using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Laso.AdminPortal.DependencyResolution.Extensions;
using Laso.AdminPortal.Infrastructure.Secrets;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation.Behaviors;
using MediatR;
using MediatR.Pipeline;
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
                });
        }

        private static void Initialize(ServiceRegistry x)
        {
            x.Scan(scan =>
            {
                scan.Assembly("Laso.AdminPortal.Infrastructure");
                scan.WithDefaultConventions();

                // Mediator
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
            });

            x.ConfigureMediator();

            x.For<IJsonSerializer>().Use<NewtonsoftSerializer>();
            x.For<IApplicationSecrets>().Use<AzureApplicationSecrets>();
            x.For<IDataQualityPipelineRepository>().Use<InMemoryDataQualityPipelineRepository>().Singleton();
        }
    }

    internal static class ServiceRegistryExtensions
    {

        internal static ServiceRegistry ConfigureMediator(this ServiceRegistry _)
        {
            //Pipeline gets executed in order
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(LoggingPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ExceptionPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ValidationPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));

            _.For<IMediator>().Use<Mediator>();
            _.For<ServiceFactory>().Use(ctx => ctx.GetInstance);

            return _;
        }
    }
}
