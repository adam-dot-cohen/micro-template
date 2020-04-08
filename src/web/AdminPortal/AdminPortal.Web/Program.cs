using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Laso.AdminPortal.Web.Configuration;
using Laso.AdminPortal.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.AdminPortal.Web
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var hostBuilderConfiguration = GetHostBuilderConfiguration(args);
            LoggingConfig.Configure(hostBuilderConfiguration);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(hostBuilderConfiguration).Build().RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application start-up failed");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(IConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    // Configure simple configuration for use during the host build process and
                    // in ConfigureAppConfiguration (or wherever the HostBuilderContext is
                    // supplied in the Host build process).
                    builder.AddConfiguration(configuration);
                })
                .ConfigureCustomDependencyResolution(configuration)
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                        return;

                    var serviceUrl = context.Configuration["Services:AdminPortal:ConfigurationSecrets:ServiceUrl"];
                    builder.AddAzureKeyVault(new Uri(serviceUrl), new DefaultAzureCredential());
                })
                .ConfigureWebHostDefaults(webBuilder => 
                    webBuilder.UseStartup<Startup>());

        public static IConfiguration GetHostBuilderConfiguration(string[] args)
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
