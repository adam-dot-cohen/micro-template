using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Testing
{
    public class HostTestFixture 
    {
        private readonly Lazy<IHost> _host;

        public HostTestFixture()
        {
            _host = new Lazy<IHost>(BuildDefaultHost);
        }

        public HostTestFixture(IHostBuilder hostBuilder)
        {
            _host = new Lazy<IHost>(BuildHost(hostBuilder));
        }

        public IHost Host => _host.Value;
        public IServiceProvider Services => Host.Services;

        public static void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureHostConfiguration(hostConfigurationBuilder => 
                hostConfigurationBuilder.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("testEnvironment", "xUnit") }));
        }

        private static IHost BuildDefaultHost()
        {
            var hostBuilder = new HostBuilder();
            ConfigureHost(hostBuilder);
            var host = hostBuilder.Build();
            host.Start();
            return host;
        }

        private static IHost BuildHost(IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            host.Start();
            return host;
        }
    }
}