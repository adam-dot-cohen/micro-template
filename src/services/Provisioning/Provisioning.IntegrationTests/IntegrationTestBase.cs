using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Laso.Provisioning.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Laso.Provisioning.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        private readonly Lazy<IHost> _host;
        private readonly Func<IConfigurationBuilder> _getConfigurationBuilder;
        private readonly List<Action<IConfigurationBuilder>> _testConfigActions;

        protected IntegrationTestBase()
            : this(DefaultConfigurationBuilder)
        {
        }

        protected IntegrationTestBase(Func<IConfigurationBuilder> testConfigurationBuilder)
        {
            _host = new Lazy<IHost>(BuildHost);
            _getConfigurationBuilder = testConfigurationBuilder;
            _testConfigActions = new List<Action<IConfigurationBuilder>>();
        }

        protected IHost Host => _host.Value;
        protected IServiceProvider Services => Host.Services;
        protected IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

        protected void ConfigureTestConfiguration(Action<IConfigurationBuilder> testConfigAction)
        {
            _testConfigActions.Add(testConfigAction);
        }

        private static IConfigurationBuilder DefaultConfigurationBuilder()
        {
            var workingDirectory = 
                Path.GetDirectoryName(Assembly.GetAssembly(typeof(IntegrationTestBase)).Location);

            var rootDirectory = $@"{workingDirectory}/../../../../../..";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($@"{rootDirectory}/services/Provisioning/Provisioning.Api/appsettings.json")
                .AddJsonFile("test.appsettings.json");

            return builder;
        }

        private IHost BuildHost()
        {
            var configuration = GetHostBuilderConfiguration();
            var hostBuilder = Program.CreateHostBuilder(configuration);
            return hostBuilder.Build();
        }

        private IConfiguration GetHostBuilderConfiguration()
        {
            var builder = _getConfigurationBuilder();

            // Add custom test configuration actions
            _testConfigActions.ForEach(a => a(builder));

            builder
                .AddUserSecrets<Startup>()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("testEnvironment", "xUnit") });

            var configuration = builder.Build();

            return configuration;
        }
    }
}
