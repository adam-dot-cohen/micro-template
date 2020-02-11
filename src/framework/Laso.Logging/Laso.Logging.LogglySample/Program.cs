using System;
using System.IO;
using Laso.Logging.Configuration;
using Laso.Logging.Loggly;
using Microsoft.Extensions.Configuration;

namespace Laso.Logging.LogglySample
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
            
            var logglySettings = new LogglySettings();
            configuration.GetSection("Laso:Logging:Loggly").Bind(logglySettings);
            

            var serilogConfig = new LoggingConfigurationBuilder()
                .BindTo(new LoglySinkBinder(loggingSettings,logglySettings,null))
                .Build();


            var log = new LogService(serilogConfig);

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
