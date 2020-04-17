using System;
using System.IO;
using Grpc.Net.Client;
using Laso.Identity.Api;
using Laso.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Identity.FunctionalTests
{
    public abstract class FunctionalTestBase
    {
        private readonly Lazy<IHostBuilder> _hostBuilder;

        private readonly Lazy<HostTestFixture> _hostFixture;
        private readonly Lazy<GrpcTestFixture> _grpcFixture;

        protected FunctionalTestBase()
        {
            _hostFixture = new Lazy<HostTestFixture>(() => new HostTestFixture(_hostBuilder.Value));
            _grpcFixture = new Lazy<GrpcTestFixture>(() => new GrpcTestFixture(_hostFixture.Value));

            _hostBuilder = new Lazy<IHostBuilder>(CreateHostBuilder(
                HostTestFixture.ConfigureHost, 
                GrpcTestFixture.ConfigureHost));
        }
        
        public IHost Host => _hostFixture.Value.Host;
        public IServiceProvider Services => _hostFixture.Value.Services;
        public GrpcChannel Channel => _grpcFixture.Value.Channel;
        
        private static IHostBuilder CreateHostBuilder(params Action<IHostBuilder>[] fixtureConfigurations)
        {
            var hostConfiguration = GetHostConfiguration();
            var hostBuilder = Program.CreateHostBuilder(hostConfiguration);

            Array.ForEach(fixtureConfigurations, c => c(hostBuilder));

            hostBuilder.ConfigureAppConfiguration(configurationBuilder =>
                configurationBuilder.AddJsonFile("appsettings.Test.json"));

            return hostBuilder;
        }

        private static IConfiguration GetHostConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Laso.Identity.Api.appsettings.json")
                .AddJsonFile("appsettings.Test.json");
            
            return builder.Build();
        }

    }
}
