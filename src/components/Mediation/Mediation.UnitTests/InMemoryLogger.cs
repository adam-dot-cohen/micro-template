using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Mediation.UnitTests
{
    public class InMemoryLogger<T> : ILogger<T>
    {
        public List<LogMessage> LogMessages { get; set; } = new List<LogMessage>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogMessages.Add(new LogMessage
            {
                LogLevel = logLevel,
                EventId = eventId,
                Exception = exception,
                Message = formatter(state, exception)
            });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullDisposable();
        }

        public class LogMessage
        {
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public Exception Exception { get; set; }
            public string Message { get; set; }
        }

        private class NullDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}