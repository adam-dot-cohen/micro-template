using System;
using System.IO;
using System.Reflection;
using Laso.AdminPortal.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Laso.AdminPortal.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        protected IntegrationTestBase()
        {
            var workingDirectory = 
                Path.GetDirectoryName(Assembly.GetAssembly(typeof(IntegrationTestBase)).Location);

            var rootDirectory = $@"{workingDirectory}\..\..\..\..\..\..";
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($@"{rootDirectory}\web\AdminPortal\AdminPortal.Web\appsettings.json")
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var hostBuilder = Program.CreateHostBuilder(configuration, null);
            var host = hostBuilder.Build();

            Services = host.Services;
            Configuration = host.Services.GetRequiredService<IConfiguration>();
        }

        protected IServiceProvider Services { get; }
        protected IConfiguration Configuration { get; }
    }
}
