using System;

namespace Infrastructure.Mediation.Internals.Extensions
{
    internal static class ExceptionExtensions
    {
        internal static Exception InnermostException(this Exception exception)
        {
            if (exception == null)
                return null;

            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception;
        }
    }
}
