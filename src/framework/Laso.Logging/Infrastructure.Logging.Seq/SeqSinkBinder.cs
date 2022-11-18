using System;
using Infrastructure.Logging.Configuration;
using Serilog;

namespace Infrastructure.Logging.Seq
{
    public class SeqSinkBinder: ILoggingSinkBinder
    {
        private readonly SeqSettings _settings;

        public SeqSinkBinder(SeqSettings settings)
        {
            _settings = settings;
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
            if (!_settings.Enabled)
                return;

            x.WriteTo
                .Seq(_settings.SeqHostUrl?? "http://localhost:5341");

        };
    }
}