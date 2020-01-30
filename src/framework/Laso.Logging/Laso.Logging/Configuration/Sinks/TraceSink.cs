using System;
using Serilog;

namespace Laso.Logging.Configuration.Sinks
{
    public class TraceSink: ILoggingSinkBinder
    {
        public Action<LoggerConfiguration> Bind => x =>
        {
            x.WriteTo.Trace();
        };
    }
}