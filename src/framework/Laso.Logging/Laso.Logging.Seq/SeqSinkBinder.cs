using System;
using Laso.Logging.Configuration;
using Serilog;

namespace Laso.Logging.Seq
{
    public class SeqSinkBinder: ILoggingSinkBinder
    {
        private readonly SeqSettings _settings;
        private readonly bool _enabled;

        public SeqSinkBinder(SeqSettings settings)
        {
            _settings = settings;
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
            if (!_settings.Enabled)
                return;

            x.WriteTo
                .Seq(_settings.SeqHostUrl?? "http://localhost:5341", compact:true);

        };
    }
}