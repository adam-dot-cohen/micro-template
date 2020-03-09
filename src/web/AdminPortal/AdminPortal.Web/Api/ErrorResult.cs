using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Laso.Logging.Extensions;

namespace Laso.AdminPortal.Web.Api
{
    public class ErrorResult
    {
        public ErrorResult(string message)
        {
            Message = message;
        }

        public string Message { get; }
        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();

        public void Add(string key, string message)
        {
            ValidationMessages.Add(new ValidationMessage(key, message));
        }
    }

    public class ValidationMessage
    {
        public ValidationMessage(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; }
        public string Message { get; }
    }

    public static class RpcExceptionExtensions
    {
        // TODO: Consider something like having a custom trailer for holding all validation messages
        // See: https://github.com/grpc/grpc/blob/master/src/csharp/Grpc.IntegrationTesting/CustomErrorDetailsTest.cs
        private static readonly string[] IgnoredTrailers = { "date", "server", "transfer-encoding" };

        public static ErrorResult ToErrorResult(this RpcException exception)
        {
            var error = new ErrorResult(exception.Status.Detail ?? "Unknown error");
            exception.Trailers
                .Where(t => !IgnoredTrailers.Contains(t.Key))
                .ForEach(t => error.Add(t.Key, t.Value));

            return error;
        }
    }
}