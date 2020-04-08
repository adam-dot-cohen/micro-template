using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Laso.Logging.Extensions;
using Laso.Provisioning.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Provisioning.Api
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configuration = GetConfiguration(args);

            LoggingConfig.Configure(configuration);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(configuration).Build().RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Start-up failed");
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
        public static IHostBuilder CreateHostBuilder(IConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder => 
                {
                    // Configure simple configuration for use during the host build process and
                    // in ConfigureAppConfiguration (or wherever the HostBuilderContext is
                    // supplied in the Host build process).
                    builder.AddConfiguration(configuration);
                })
                .UseSerilog()
                .ConfigureAppConfiguration(
                    (context, builder) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                            return;

                        var serviceUrl = context.Configuration["Services:Provisioning:ConfigurationSecrets:ServiceUrl"];
                        builder.AddAzureKeyVault(new Uri(serviceUrl), new DefaultAzureCredential());
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>());
    
        public static IConfiguration GetConfiguration(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

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
    }
}
