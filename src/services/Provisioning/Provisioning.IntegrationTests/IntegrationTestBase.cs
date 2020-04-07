using System;
using System.IO;
using System.Reflection;
using Laso.Provisioning.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Laso.Provisioning.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        protected IntegrationTestBase()
        {
            var workingDirectory = 
                Path.GetDirectoryName(Assembly.GetAssembly(typeof(IntegrationTestBase)).Location);

            var rootDirectory = $@"{workingDirectory}/../../../../../..";
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($@"{rootDirectory}/services/Provisioning/Provisioning.Api/appsettings.json")
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var host = Program.CreateHostBuilder(configuration).Build();

            Services = host.Services;
            Configuration = host.Services.GetRequiredService<IConfiguration>();
        }

        protected IServiceProvider Services { get; }
        protected IConfiguration Configuration { get; }
    }
}
