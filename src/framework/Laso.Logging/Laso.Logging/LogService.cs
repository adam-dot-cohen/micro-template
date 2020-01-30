using System;
using System.Collections.Generic;
using System.Linq;
using Laso.Logging.Configuration;
using Laso.Logging.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;

namespace Laso.Logging
{
    public class LoggingConfiguration
    {
        public IDictionary<LogLevel, bool> LogLevels = new Dictionary<LogLevel, bool>();
        public IList<Action> ShutDownActions = new List<Action>();

        internal LoggingConfiguration(IList<ILogEventEnricher> enrichers, IList<ILoggingSinkBinder> binders,
            IDictionary<LogLevel, bool> levels)
        {                           
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug();

            logConfig.Enrich.With(enrichers.ToArray());

            foreach (var loggingSinkBinder in binders)
            {
                loggingSinkBinder.Bind(logConfig);
            }
            SelfLog.Enable(Console.Error);
            Log.Logger = logConfig.CreateLogger();
            ShutDownActions.Add(Log.CloseAndFlush);

            foreach (var level in (LogLevel[]) Enum.GetValues(typeof(LogLevel)))
            {
                LogLevels[level] = !LogLevels.ContainsKey(level) || LogLevels.ContainsKey(level);
            }

        }
    }

    public class LogService: ILogService
    {
        private readonly LoggingConfiguration _config;

        public LogService(LoggingConfiguration config)
        {
            _config = config;
        }

        public IDisposable Stopwatch(string operation, object context = null, ILogContext logContext = null,
            ILogService logService = null)
        {
            return new NOP.NopDisposable();
        }

        public void Debug(string messageTemplate, params object[] arguments)
        {

            if (_config.LogLevels[LogLevel.Debug])
            {
                Log.Logger.Debug(messageTemplate,arguments);
            }

        }

        public void Debug(Exception exception, string messageTemplate, params object[] arguments)
        { 
            if (_config.LogLevels[LogLevel.Debug])
            {
                Log.Logger.Debug(exception, messageTemplate,arguments);

            }
        }

        public void Information(string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Information])
            {
                Log.Logger.Information( messageTemplate,arguments);
            }
        }

        public void Information(Exception exception, string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Information])
            {
                
                Log.Logger.Information(exception, messageTemplate,arguments);
            }
        }

        public void Warning(string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Warning])
            {
                
                Log.Logger.Warning( messageTemplate,arguments);
            }
        }

        public void Warning(Exception exception, string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Warning])
            {
                
                Log.Logger.Warning(exception, messageTemplate,arguments);
            }
        }

        public void Error(string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Error])
            {
                
                Log.Logger.Error( messageTemplate,arguments);
            }
        }

        public void Error(Exception exception, string messageTemplate, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Error])
            {
                
                Log.Logger.Error(exception, messageTemplate,arguments);
            }
        }

        public void Exception(Exception exception, params object[] arguments)
        {
            if (_config.LogLevels[LogLevel.Exception])
            {
                var errorMessage = $"{exception?.InnermostException().Message ?? "Unknown error"}";
                if (arguments?.Length > 0)
                {
                    Log.Logger.Error(exception, $"{errorMessage} {{@ErrorData}}", arguments);
                }
                else
                {
                    Log.Logger.Error(exception, errorMessage);
                }
            }
        }

        public void ShutDown()
        {
            _config.ShutDownActions.ForEach(x=>x.Invoke());
        }
    }
}
