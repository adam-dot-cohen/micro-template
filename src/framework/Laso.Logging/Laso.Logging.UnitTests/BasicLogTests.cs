using System;
using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Sinks;
using Serilog.Formatting.Display;
using Xunit;
using Xunit.Abstractions;

namespace Laso.Logging.UnitTests
{
   public  class BasicLogTests
   {

        private readonly ITestOutputHelper _helper;   

        public BasicLogTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }


        [Fact]
        public void should_log()
        {
            var configuration = new LoggingConfigurationBuilder()
                .BindTo(new XUnitSinkBinder(_helper,new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")))
                .Build();

            var service = new LogService(configuration);
            
            service.Debug("test {0}","Value 1");
            service.Information("test {0}","Value 2");
            service.Warning("test {0}","Value 3");
            service.Error("test {0}","Value 4");
            service.Exception(new Exception("test {0}"),"Value 5");

        }

    }
}
