using System;

namespace Infrastructure.Logging
{
    public interface ILogService
    {
        void Debug(string messageTemplate, params object[] arguments);

        void Debug(Exception exception, string messageTemplate, params object[] arguments);

        void Information(string messageTemplate, params object[] arguments);

        void Information(Exception exception, string messageTemplate, params object[] arguments);

        void Warning(string messageTemplate, params object[] arguments);

        void Warning(Exception exception, string messageTemplate, params object[] arguments);

        void Error(string messageTemplate, params object[] arguments);

        void Error(Exception exception, string messageTemplate, params object[] arguments);

        void Exception(Exception exception, params object[] data);

        void ShutDown();
    }
}