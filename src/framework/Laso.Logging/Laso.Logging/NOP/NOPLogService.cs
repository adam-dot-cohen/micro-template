using System;

namespace Laso.Logging.NOP
{
    public class NOPLogService:ILogService
    {
        public IDisposable Stopwatch(string operation, object context = null, ILogContext logContext = null,
            ILogService logService = null)
        {
            return new NopDisposable();
        }

        public void Debug(string messageTemplate, params object[] arguments)
        {
        }

        public void Debug(Exception exception, string messageTemplate, params object[] arguments)
        {
        }

        public void Information(string messageTemplate, params object[] arguments)
        {
        }

        public void Information(Exception exception, string messageTemplate, params object[] arguments)
        {
        }

        public void Warning(string messageTemplate, params object[] arguments)
        {
        }

        public void Warning(Exception exception, string messageTemplate, params object[] arguments)
        {
        }

        public void Error(string messageTemplate, params object[] arguments)
        {
        }

        public void Error(Exception exception, string messageTemplate, params object[] arguments)
        {
        }

        public void Exception(Exception exception, params object[] data)
        {
        }

        public void ShutDown()
        {
            
        }
    }
}
