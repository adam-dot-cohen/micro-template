using System;
using System.IO;
using Laso.Logging.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Xunit.Abstractions;

namespace Laso.Logging.UnitTests
{
    public class XUnitSinkBinder: ILoggingSinkBinder
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ITextFormatter _textFormatter;

        public XUnitSinkBinder(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter)
        {
            _testOutputHelper = testOutputHelper ;
            _textFormatter = textFormatter ;
        }
        

        public void Emit(LogEvent logEvent)
        {

            var writer = new StringWriter();
            _textFormatter.Format(logEvent, writer);
            _testOutputHelper.WriteLine(writer.ToString().Trim());
        }

        public Action<LoggerConfiguration> Bind => x =>
        {
            var restrictedToMinimumLevel = new XUnitSink(_testOutputHelper,_textFormatter);
            x.WriteTo.Sink(restrictedToMinimumLevel, LogEventLevel.Verbose);
        };

        internal class XUnitSink:ILogEventSink
        { 
            
            private readonly ITestOutputHelper _testOutputHelper;
            private readonly ITextFormatter _textFormatter;
            public XUnitSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter)
            {
                _testOutputHelper = testOutputHelper;
                _textFormatter = textFormatter;
            }

            public void Emit(LogEvent logEvent)
            {

            
                var renderSpace = new StringWriter();
                _textFormatter.Format(logEvent, renderSpace);
                _testOutputHelper.WriteLine(renderSpace.ToString().Trim());
            }
        }

    }
}
