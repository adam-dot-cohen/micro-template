using System;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview.Extensions
{
    internal static class ExceptionExtensions
    {
        public static Exception InnermostException(this Exception exception)
        {
            if (exception == null)
                return null;

            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception;
        }
    }
}
