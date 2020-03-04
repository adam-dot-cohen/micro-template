using System;
using System.Linq;
using System.Threading.Tasks;
using Laso.Identity.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
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
            LoggingConfig.Configure();

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(args).Build().RunAsync();
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
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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

                        Log.Information($"Using Azure KeyVault: {builtConfig["AzureKeyVault:VaultBaseUrl"]}");
                        config.AddAzureKeyVault(
                            builtConfig["AzureKeyVault:VaultBaseUrl"],
                            keyVaultClient,
                            new LasoKeyVaultSecretManager());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        // .UseKestrel()
                        // .UseUrls("http://localhost:59418")
                        // .UseContentRoot(Directory.GetCurrentDirectory())
                        // .UseIISIntegration()
                        .UseStartup<Startup>();
                })
        ;
    }

    public class LasoKeyVaultSecretManager : IKeyVaultSecretManager
    {
        /// <inheritdoc />
        public virtual string GetKey(SecretBundle secret)
        {
            var key = secret.SecretIdentifier.Name.Replace("--", ConfigurationPath.KeyDelimiter);
            Log.Information("Loading KeyVault Key {@KeyVaultKeyName}", key);
            return key;
        }

        /// <inheritdoc />
        public virtual bool Load(SecretItem secret)
        {
            return true;
        }
    }

}
