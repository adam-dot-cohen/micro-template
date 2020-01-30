using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Laso.Logging
{
    public class LoggingStopwatch : IDisposable
    {
        private readonly string _operationName;
        private readonly object _context;
        private readonly ILogContext _logContext;
        private readonly ILogService _logService;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Log the elapsed time for a given operation.
        /// </summary>
        /// <param name="operationName">Application-wide unique operation name</param>
        /// <param name="context">An anonymous object containing contextual information to log</param>
        /// <param name="logContext">If awaiting async methods within the LoggingStopwatch using block,  provide the current logContext to have it preserved in the stopwatch log entries.</param>
        /// <param name="logService">Log Wrapper.</param>
        public LoggingStopwatch(string operationName, object context = null, ILogContext logContext = null, ILogService logService = null)
        {
            _operationName = operationName;
            _context = context;
            _logService = logService ;
            //TODO - GO back and look at what's happening here and see if it's necessary any more if we move to all instance based.
            if (logContext != null)
            //    _logContext = logContext.ToStaticLogContext();

            LogOperationExecuting();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        private void LogOperationExecuting()
        {
            if (_logService == null)
                return;

            var operation = GetOperationStarted();

            if (_logContext == null)
                _logService.Debug("Executing {@Operation}", operation);
            else
                _logService.Debug("Executing {@Operation} {@LogContext}", operation, _logContext);
            
            Trace.Indent();
        }

        private object GetOperationStarted()
        {
            if (_context == null)
                return new { Name = _operationName, Status = "Started" };
            
            return new { Name = _operationName, Context = _context, Status = "Started" };
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            LogOperationExecuted();
        }

        private void LogOperationExecuted()
        {
            if (_logService == null)
                return;
            
            Trace.Unindent();
            
            var operation = GetOperationCompleted();
            
            if (_logContext == null)
                _logService.Information("Executed {@Operation}", operation);
            else
                _logService.Information("Executed {@Operation} {@LogContext}", operation, _logContext);
        }

        private object GetOperationCompleted()
        {
            if (_context == null)
                return new { Name = _operationName, Status = "Completed", _stopwatch.Elapsed.TotalSeconds };
            
            return new { Name = _operationName, Context = _context, Status = "Completed", _stopwatch.Elapsed.TotalSeconds };
        }
    }
}
