using System;
using System.IO;
using System.Threading.Tasks;
using Laso.Provisioning.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Provisioning.Api
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configuration = GetConfiguration();

            LoggingConfig.Configure(configuration);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(configuration, args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Start-up failed");
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
        public static IHostBuilder CreateHostBuilder(IConfiguration configuration, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));

                        builder.AddAzureKeyVault(
                            configuration["AzureKeyVault:VaultBaseUrl"],
                            keyVaultClient,
                            new DefaultKeyVaultSecretManager());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>());
    
        public static IConfiguration GetConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return configuration;
        }
    }
}
