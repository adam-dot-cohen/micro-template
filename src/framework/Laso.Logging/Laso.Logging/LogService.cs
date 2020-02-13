using System;
using Laso.Logging.Configuration;
using Laso.Logging.Extensions;
using Serilog;

namespace Laso.Logging
{
    public class LogService: ILogService
    {
        private readonly LoggingConfiguration _config;

        public LogService(LoggingConfiguration config)
        {
            _config = config;
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
