using Infrastructure.Logging.Configuration;
using Infrastructure.Logging.Configuration.Enrichers;
using Serilog.Configuration;

namespace Infrastructure.Logging.Extensions
{
    public static class InfrastructureEnricherExtensions
    {
        public static LoggerEnrichmentConfiguration ForInfrastructure(this LoggerEnrichmentConfiguration config, LoggingSettings commonSettings)
        {
            InfrastructureEnricher.CommonSettings = commonSettings;
            config.With<InfrastructureEnricher>();
            return config;
        }
    }
}