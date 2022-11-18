using System;
using System.IO;
using Infrastructure.Logging.Configuration;
using Infrastructure.Logging.Elasticsearch;
using Infrastructure.Logging.Seq;
using Infrastructure.Logging.Extensions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Logging.SeqSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            var loggingSettings = new LoggingSettings();
            configuration.GetSection("Infrastructure:Logging:Common").Bind(loggingSettings);

            var seqSettings = new SeqSettings();
            configuration.GetSection("Infrastructure:Logging:Seq").Bind(seqSettings);

            var elasticSearchSettings = new ElasticsearchSettings();
            configuration.GetSection("Infrastructure:Logging:Elasticsearch").Bind(seqSettings);


            var lc = new LoggingConfigurationBuilder()
                .BindTo(new SeqSinkBinder(seqSettings))
                .BindTo(new ElasticsearchSinkBinder(elasticSearchSettings))
                .Build(x => x.Enrich.ForInfrastructure(loggingSettings));

            var log = new LogService(lc);


            log.Debug("test {0}","Value 1");
            log.Information("test {0}","Value 2");
            log.Warning("test {0}","Value 3");
            log.Error("test {0}","Value 4");
            log.Exception(new Exception("test"),"Value 5");

            Console.WriteLine("\n Press any key to close");
            Console.ReadLine();
        }
    }
}
