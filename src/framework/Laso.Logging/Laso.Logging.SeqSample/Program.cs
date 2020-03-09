using System;
using System.IO;
using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Enrichers;
using Laso.Logging.Extensions;
using Laso.Logging.Seq;
using Microsoft.Extensions.Configuration;

namespace Laso.Logging.SeqSample
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
            configuration.GetSection("Laso:Logging:Common").Bind(loggingSettings);

            var seqSettings = new SeqSettings();
            configuration.GetSection("Laso:Logging:Seq").Bind(seqSettings);


            var lc = new LoggingConfigurationBuilder()
                .BindTo(new SeqSinkBinder(seqSettings))
                .Build(x => x.Enrich.ForLaso(loggingSettings));

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
