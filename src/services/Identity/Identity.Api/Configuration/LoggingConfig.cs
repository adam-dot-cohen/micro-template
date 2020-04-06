﻿using Laso.Logging.Configuration;
using Laso.Logging.Extensions;
using Laso.Logging.Loggly;
using Laso.Logging.Seq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Laso.Identity.Api.Configuration
{
    public static class LoggingConfig
    {
        public static void Configure(IConfiguration config)
        {
            // Get settings
            var loggingSettings = config.GetSection("Laso:Logging:Common").Get<LoggingSettings>();
            var seqSettings = config.GetSection("Laso:Logging:Seq").Get<SeqSettings>();
            var logglySettings = config.GetSection("Laso:Logging:Loggly").Get<LogglySettings>();

            // Enrich
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .MinimumLevel.Override("Grpc", LogEventLevel.Debug)
                .Enrich.FromLogContext();
            logConfig.Enrich.ForLaso(loggingSettings);

            // Configure
            ConfigureConsole(logConfig);
            ConfigureSeq(logConfig, seqSettings);
            ConfigureLoggly(loggingSettings, logglySettings, logConfig);

            Log.Logger = logConfig.CreateLogger();
        }

        private static void ConfigureConsole(LoggerConfiguration logConfig)
        {
            logConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Literate);
        }

        private static void ConfigureSeq(LoggerConfiguration logConfig, SeqSettings seqSettings)
        {
            new SeqSinkBinder(seqSettings).Bind(logConfig);
        }

        private static void ConfigureLoggly(LoggingSettings loggingSettings, LogglySettings logglySettings, LoggerConfiguration logConfig)
        {
            new LogglySinkBinder(loggingSettings, logglySettings).Bind(logConfig);
        }
    }
}