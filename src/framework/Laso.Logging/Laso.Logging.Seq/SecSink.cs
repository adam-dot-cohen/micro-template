using System;
using Laso.Logging.Seq;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Laso.Logging.Configuration.Sinks
{
    public class SeqSink: ILoggingSinkBinder
    {
        private readonly IHttpContextAccessor _accessor;
        
        private readonly bool _enabled;
        private readonly string _environment;
        private readonly string _application;
        private readonly string _version;
        private readonly string _tenantName;

        public SeqSink(IHttpContextAccessor accessor, bool enabled, string environment, string application, string version, string tenantName)
        {
            _accessor = accessor;
            _enabled = enabled;
            _environment = environment;
            _application = application;
            _version = version;
            _tenantName = tenantName;
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
            if (!_enabled)
                return;

            x.WriteTo
                .Seq("http://localhost:5341", compact:true)                
                .Enrich.With(new SeqEnricher(_accessor,_environment,_application,_version,_tenantName));

        };
    }
}