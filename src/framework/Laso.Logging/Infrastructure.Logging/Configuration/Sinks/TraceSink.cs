using System;
using Serilog;

namespace Infrastructure.Logging.Configuration.Sinks
{
    public class TraceSink: ILoggingSinkBinder
    {
        public Action<LoggerConfiguration> Bind => x =>
        {
            x.WriteTo.Trace();
        };
    }
}