using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Core;

namespace Laso.Logging.Configuration
{

    public enum LogLevel
    {
        Debug=1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Exception =5
    }

    public interface ILoggingSinkBinder
    {
        Action<LoggerConfiguration> Bind { get; }
    }

    public class LoggingConfigurationBuilder
    {
        private readonly IList<ILogEventEnricher> _enrichers = new List<ILogEventEnricher>();
        private readonly IList<ILoggingSinkBinder> _binders = new List<ILoggingSinkBinder>();
        private IDictionary<LogLevel,bool> _levels = new Dictionary<LogLevel,bool>();


        public LoggingConfigurationBuilder EnrichWith(ILogEventEnricher enricher)
        {
            _enrichers.Add(enricher);
            return this;
        }
        public LoggingConfigurationBuilder BindTo(ILoggingSinkBinder enricher)
        {
            _binders.Add(enricher);
            return this;
        }

        public LoggingConfigurationBuilder WithLoggingLevel(LogLevel level, bool enabled)
        {
            _levels[level] = enabled;
            return this;
        }



        public LoggingConfiguration Build()
        {
            var loggingConfiguration = new LoggingConfiguration(_enrichers,_binders,_levels);
            return loggingConfiguration;
        }

    }
}
