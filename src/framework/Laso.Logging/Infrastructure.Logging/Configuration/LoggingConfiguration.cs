using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Debugging;

namespace Infrastructure.Logging.Configuration
{
    public class LoggingConfiguration
    {
        public readonly LoggerConfiguration LogConfig;
        public IDictionary<LogLevel, bool> LogLevels = new Dictionary<LogLevel, bool>();
        public IList<Action> ShutDownActions = new List<Action>();

        internal LoggingConfiguration(IList<ILoggingSinkBinder> binders, IDictionary<LogLevel, bool> levels,
            Action<LoggerConfiguration> additionalConfiguration)
        {
            LogConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext();

            foreach (var loggingSinkBinder in binders) loggingSinkBinder.Bind(LogConfig);

            additionalConfiguration?.Invoke(LogConfig);

            SelfLog.Enable(Console.Error);
            ShutDownActions.Add(Log.CloseAndFlush);

            Log.Logger = LogConfig.CreateLogger();

            foreach (var level in (LogLevel[]) Enum.GetValues(typeof(LogLevel)))
                LogLevels[level] = !levels.ContainsKey(level) || levels[level];
        }
    }
}