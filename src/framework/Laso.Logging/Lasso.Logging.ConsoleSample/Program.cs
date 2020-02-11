using System;
using Laso.Logging;
using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Sinks;

namespace Lasso.Logging.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new LoggingConfigurationBuilder()
                .BindTo(new ConsoleSink(true))
                .BindTo(new TraceSink())
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
