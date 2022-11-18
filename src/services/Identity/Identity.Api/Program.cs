using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lamar.Microsoft.DependencyInjection;
using Laso.Hosting;
using Laso.Hosting.Extensions;
using Laso.Identity.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Identity.Api
{
    public class Program : IProgram
    {
        public static async Task<int> Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = GetConfiguration(args, environment);

           //todo //todo LoggingConfig.Configure(configuration);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(configuration).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(IConfiguration hostConfiguration) =>
            Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    // Configure simple configuration for use during the host build process and
                    // in ConfigureAppConfiguration (or wherever the HostBuilderContext is
                    // supplied in the Host build process).
                    builder.AddConfiguration(hostConfiguration);
                })
                .UseLamar()
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var serviceUrl = context.Configuration["Services:Identity:ConfigurationSecrets:ServiceUrl"];
                    builder.AddAzureKeyVault(serviceUrl, context);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-3.1#specify-the-hosting-startup-assembly
                        .UseSetting(WebHostDefaults.HostingStartupAssembliesKey, hostConfiguration["DependencyResolution:ConfigurationAssembly"])
                        .UseStartup<Startup>();
                });

        private static IConfiguration GetConfiguration(string[] args, [CanBeNull] string environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            // Add this for development/testing -- allows secrets to be retrieved for
            // key vault from local user secret store (see AddAzureKeyVault)
            if (string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase))
                builder.AddUserSecrets<Startup>();

            var configuration = builder.Build();

            return configuration;
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        IConfiguration IProgram.GetConfiguration(string[] args, string environment)
        {
            return GetConfiguration(args, environment);
        }

        IHostBuilder IProgram.CreateHostBuilder(IConfiguration configuration)
        {
            return CreateHostBuilder(configuration);
        }
    }
}
