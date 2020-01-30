using System;
using Serilog;

namespace Laso.Logging.Configuration.Sinks
{
    public class ConsoleSink: ILoggingSinkBinder
    {
        private readonly bool _enabled;

        public ConsoleSink(bool enabled)
        {
            _enabled = enabled;
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
            if (!_enabled)
                return;
            
            x.WriteTo.Console(outputTemplate: "{Message}{NewLine}{Exception}");

        };
    }
}
