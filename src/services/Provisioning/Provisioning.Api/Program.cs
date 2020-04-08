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
                .ConfigureHostConfiguration(configurationBuilder => 
                    configurationBuilder.AddConfiguration(configuration))
                .UseSerilog()
                .ConfigureAppConfiguration(
                    (context, builder) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                            return;

                        var serviceUrl = configuration["Services:Provisioning:ConfigurationSecrets:ServiceUrl"];
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
                .AddEnvironmentVariables();

            if (!args.IsNullOrEmpty())
                builder.AddCommandLine(args);

            var configuration = builder.Build();

            return configuration;
        }
    }
}
