using System;
using System.IO;
using System.Threading.Tasks;
using Lamar.Microsoft.DependencyInjection;
using Laso.Catalog.Api.Configuration;
using Laso.Hosting;
using Laso.Hosting.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Catalog.Api
{
    public class Program : IProgram
    {
        public static async Task<int> Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = GetConfiguration(args, environment);

            LoggingConfig.Configure(configuration);

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

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
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
                    var serviceUrl = context.Configuration["Services:Catalog:ConfigurationSecrets:ServiceUrl"];
                    builder.AddAzureKeyVault(serviceUrl, context);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                });

        public static IConfiguration GetConfiguration(string[] args, string? environment)
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

        IConfiguration IProgram.GetConfiguration(string[] args, string? environment)
        {
            return GetConfiguration(args, environment);
        }

        IHostBuilder IProgram.CreateHostBuilder(IConfiguration configuration)
        {
            return CreateHostBuilder(configuration);
        }
    }
}
