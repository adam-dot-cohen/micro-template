using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Laso.AdminPortal.DependencyResolution.Extensions;
using Laso.AdminPortal.Infrastructure.Secrets;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IO.Serialization;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation.Configuration.Lamar;
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
            x.For<IMessageBuilder>().Use<DefaultMessageBuilder>();
        }
    }
}
