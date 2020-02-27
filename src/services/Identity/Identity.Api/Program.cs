using System;
using System.Threading.Tasks;
using Laso.Identity.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        // .UseKestrel()
                        // .UseUrls("http://localhost:59418")
                        // .UseContentRoot(Directory.GetCurrentDirectory())
                        // .UseIISIntegration()
                        .UseStartup<Startup>();
                });
    }
}
