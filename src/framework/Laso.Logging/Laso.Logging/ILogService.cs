using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Logging
{

        public interface ILogService
        {
            IDisposable Stopwatch(string operation, object context = null, ILogContext logContext = null, ILogService logService = null);

            void Debug(string messageTemplate, params object[] arguments);

            void Debug(Exception exception, string messageTemplate, params object[] arguments);

            void Information(string messageTemplate, params object[] arguments);

            void Information(Exception exception, string messageTemplate, params object[] arguments);

            void Warning(string messageTemplate, params object[] arguments);

            void Warning(Exception exception, string messageTemplate, params object[] arguments);

            void Error(string messageTemplate, params object[] arguments);

            void Error(Exception exception, string messageTemplate, params object[] arguments);

            void Exception(Exception exception, params object[] data);

           
        }
}
