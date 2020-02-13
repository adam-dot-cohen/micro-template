using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Enrichers;
using Serilog.Configuration;

namespace Laso.Logging.Extensions
{
    public static class LasoEnricherExtensions
    {
        public static LoggerEnrichmentConfiguration ForLaso(this LoggerEnrichmentConfiguration config, LoggingSettings commonSettings)
        {
            LasoEnricher.CommonSettings = commonSettings;
            config.With<LasoEnricher>();
            return config;
        }
    }
}