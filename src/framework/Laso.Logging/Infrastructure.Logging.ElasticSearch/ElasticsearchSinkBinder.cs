using Infrastructure.Logging.Configuration;
using Infrastructure.Logging.Seq;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Infrastructure.Logging.Elasticsearch;

public class ElasticsearchSinkBinder:ILoggingSinkBinder
{
    private readonly ElasticsearchSettings _elasticSearchSettings;

    public ElasticsearchSinkBinder(ElasticsearchSettings elasticSearchSettings)
    {
        _elasticSearchSettings = elasticSearchSettings;
    }

    public Action<LoggerConfiguration> Bind => x =>
    {
    
        if (!_elasticSearchSettings.Enabled)
            return;

        x.WriteTo
            .Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(_elasticSearchSettings.Url))
                {
                    AutoRegisterTemplate = _elasticSearchSettings.AutoRegisterTemplate,
                    AutoRegisterTemplateVersion = _elasticSearchSettings.AutoRegisterTemplateVersion,
                    IndexFormat = _elasticSearchSettings.IndexFormat,
                });

    };

}