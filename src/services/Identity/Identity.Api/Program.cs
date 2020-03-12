using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lamar.Microsoft.DependencyInjection;
using Laso.Identity.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Identity.Api
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var baselineConfig = GetBaselineConfiguration();
            LoggingConfig.Configure(baselineConfig);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(args, baselineConfig).Build().RunAsync();
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
        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration baselineConfig) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        var builtConfig = config.Build();

                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));

                        config.AddAzureKeyVault(
                            builtConfig["AzureKeyVault:VaultBaseUrl"],
                            keyVaultClient,
                            new DefaultKeyVaultSecretManager());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-3.1#specify-the-hosting-startup-assembly
                        .UseSetting(WebHostDefaults.HostingStartupAssembliesKey, baselineConfig["DependencyResolution:ConfigurationAssembly"])
                        .UseStartup<Startup>();
                })
                .UseSerilog()
        ;

        private static IConfiguration GetBaselineConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }
    }
}
