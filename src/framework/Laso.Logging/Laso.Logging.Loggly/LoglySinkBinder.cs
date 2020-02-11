using System;
using Laso.Logging.Configuration;
using Loggly;
using Loggly.Config;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Laso.Logging.Loggly
{
    public class LoglySinkBinder:ILoggingSinkBinder
    {
        private readonly LoggingSettings _commonSettings;
        private readonly LogglySettings _logglySettings;
        private readonly IHttpContextAccessor _accessor;

        public LoglySinkBinder(LoggingSettings commonSettings, LogglySettings logglySettings, IHttpContextAccessor accessor)
        {
            _commonSettings = commonSettings;
            _logglySettings = logglySettings;
            _accessor = accessor;

            SetupLogglyConfiguration(_commonSettings,_logglySettings);
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
    
            if (!_logglySettings.Enabled)
                return;

            x.WriteTo
                .Loggly(
                    batchPostingLimit: _logglySettings.MaxBatchSize,
                    period: TimeSpan.FromSeconds(_logglySettings.BatchPeriodSeconds))
                .Enrich.With(new LogglyEnricher(_accessor,_commonSettings.Environment,_commonSettings.Application,_commonSettings.Version,_commonSettings.TenantName));
      

            

        };

        private static void SetupLogglyConfiguration(LoggingSettings commonSettings, LogglySettings logglySettings)
        {
            //Configure Loggly
            var config = LogglyConfig.Instance;
            config.CustomerToken = logglySettings.CustomerToken;
            config.ApplicationName = commonSettings.Application;
            // config.Transport = new TransportConfiguration()
            // {
            //     EndpointHostname = logglySettings.EndpointHostname,
            //     EndpointPort = logglySettings.EndpointPort,
            //     LogTransport = logglySettings.LogTransport
            // };
            // config.ThrowExceptions = logglySettings.ThrowExceptions;
            //
            // //Define Tags sent to Loggly
            // config.TagConfig.Tags.AddRange(new ITag[]{
            //     new ApplicationNameTag {Formatter = "Application-{0}"},
            //     new HostnameTag { Formatter = "Host-{0}" }
            // });
        }
    }
}