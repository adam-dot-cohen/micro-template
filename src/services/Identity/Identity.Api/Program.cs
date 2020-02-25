using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Laso.Identity.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("grpc", LogEventLevel.Debug)
                .Enrich.FromLogContext();
            ConfigureConsole(logConfig);
            ConfigureSeq(logConfig);

            Log.Logger = logConfig.CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
                return 0;
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
        }

        private static void ConfigureConsole(LoggerConfiguration logConfig)
        {
            logConfig.WriteTo
                .Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Literate
                );
        }

        private static void ConfigureSeq(LoggerConfiguration logConfig)
        {
            var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
            if (seqUrl != null)
            {
                logConfig.WriteTo.Seq(seqUrl);
            }
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
