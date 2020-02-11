using System;
using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Sinks;
using Laso.Logging.Seq;

namespace Laso.Logging.SeqSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new LoggingConfigurationBuilder()
                .BindTo(new SeqSinkBinder(null,true,"Developer","Test","1.0.0.0",null))
                .Build();



            var log = new LogService(configuration);

            log.Debug("test {0}","Value 1");
            log.Information("test {0}","Value 2");
            log.Warning("test {0}","Value 3");
            log.Error("test {0}","Value 4");
            log.Exception(new Exception("test {0}"),"Value 5");

            Console.WriteLine("\n Press any key to close");
            Console.ReadLine();
        }
    }
}
