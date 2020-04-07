using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Laso.AdminPortal.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        private readonly Lazy<IHost> _host;
        private readonly List<Action<IConfigurationBuilder>> _testConfigActions;

        protected IntegrationTestBase()
        {
            _host = new Lazy<IHost>(BuildHost);
            _testConfigActions = new List<Action<IConfigurationBuilder>>();
        }

        protected IHost Host => _host.Value;
        protected IServiceProvider Services => Host.Services;
        protected IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

        protected void ConfigureTestConfiguration(Action<IConfigurationBuilder> testConfigAction)
        {
            _testConfigActions.Add(testConfigAction);
        }

        private IHost BuildHost()
        {
            var configuration = GetHostBuilderConfiguration();
            var hostBuilder = Program.CreateHostBuilder(configuration);
            return hostBuilder.Build();
        }

        private IConfiguration GetHostBuilderConfiguration()
        {
            var workingDirectory = 
                Path.GetDirectoryName(Assembly.GetAssembly(typeof(IntegrationTestBase)).Location);

            var rootDirectory = $@"{workingDirectory}/../../../../../..";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($@"{rootDirectory}/web/AdminPortal/AdminPortal.Web/appsettings.json")
                .AddJsonFile("test.appsettings.json");

            // Add custom test configuration actions
            _testConfigActions.ForEach(a => a(builder));

            var configuration = builder.Build();

            return configuration;
        }
    }
}
