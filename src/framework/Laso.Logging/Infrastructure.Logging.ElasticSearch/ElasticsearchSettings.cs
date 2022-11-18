using Serilog.Sinks.Elasticsearch;

namespace Infrastructure.Logging.Seq
{
    public class ElasticsearchSettings
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public bool AutoRegisterTemplate{ get; set; }
        public AutoRegisterTemplateVersion AutoRegisterTemplateVersion { get; set; }
        public string IndexFormat { get; set; }

    }
}