using System;
using System.IO;
using System.Threading.Tasks;
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
            var configuration = GetConfiguration();

            LoggingConfig.Configure(configuration);

            try
            {
                Log.Information("Starting up");
                await CreateHostBuilder(configuration, args).Build().RunAsync();
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

        public static IHostBuilder CreateHostBuilder(IConfiguration configuration, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseCustomDependencyResolution(configuration)
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var vaultUri = configuration["AzureKeyVault:VaultBaseUrl"];
                    builder.AddAzureKeyVault(vaultUri);
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
