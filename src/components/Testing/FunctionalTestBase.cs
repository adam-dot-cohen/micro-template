using System;
using System.IO;
using System.Net.Http;
using Grpc.Net.Client;
using Laso.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Testing
{
    public abstract class FunctionalTestBase<TProgram> where TProgram : IProgram, new()
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
        public HttpClient Client => _grpcFixture.Value.Client;
        public GrpcChannel Channel => _grpcFixture.Value.Channel;
        
        private static IHostBuilder CreateHostBuilder(params Action<IHostBuilder>[] fixtureConfigurations)
        {
            var program = new TProgram();

            var hostConfiguration = GetHostConfiguration();
            var hostBuilder = program.CreateHostBuilder(hostConfiguration);

            Array.ForEach(fixtureConfigurations, c => c(hostBuilder));

            hostBuilder.ConfigureAppConfiguration(configurationBuilder =>
                configurationBuilder.AddConfiguration(hostConfiguration));

            return hostBuilder;
        }

        private static IConfiguration GetHostConfiguration()
        {
            var assemblyName = typeof(TProgram).Assembly.GetName().Name;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{assemblyName}.appsettings.json")
                .AddJsonFile("appsettings.Test.json", true);
            
            return builder.Build();
        }
    }
}
