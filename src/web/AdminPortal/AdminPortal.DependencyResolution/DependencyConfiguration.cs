using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Infrastructure.KeyVault;
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
                .UseLamar(registry);
        }

        private static void Initialize(ServiceRegistry x)
        {
            x.Scan(scan =>
            {
                scan.Assembly("Laso.AdminPortal.Infrastructure");
                scan.WithDefaultConventions();

                scan.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
            });

            x.For<IApplicationSecrets>().Use<AzureApplicationSecrets>();
        }
    }
}
