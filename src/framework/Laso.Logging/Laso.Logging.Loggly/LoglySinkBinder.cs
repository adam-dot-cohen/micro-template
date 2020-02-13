using System;
using Laso.Logging.Configuration;
using Loggly;
using Loggly.Config;
using Serilog;

namespace Laso.Logging.Loggly
{
    public class LoglySinkBinder:ILoggingSinkBinder
    {
        private readonly LoggingSettings _commonSettings;
        private readonly LogglySettings _logglySettings;

        public LoglySinkBinder(LoggingSettings commonSettings, LogglySettings logglySettings)
        {
            _commonSettings = commonSettings;
            _logglySettings = logglySettings;

            SetupLogglyConfiguration();
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
    
            if (!_logglySettings.Enabled)
                return;

            x.WriteTo
                .Loggly(
                    batchPostingLimit: _logglySettings.MaxBatchSize,
                    period: TimeSpan.FromSeconds(_logglySettings.BatchPeriodSeconds));

        };

        private void SetupLogglyConfiguration( )
        {
            var config = LogglyConfig.Instance;
            config.CustomerToken = _logglySettings.CustomerToken;
            config.ApplicationName = _commonSettings.Application;

            
            config.Transport = new TransportConfiguration()
            {
                EndpointHostname = _logglySettings.HostName ?? "logs-01.loggly.com",
                LogTransport = LogTransport.Https,
                EndpointPort = _logglySettings.EndpointPort == 0?443: _logglySettings.EndpointPort
            };

        }

    }
}