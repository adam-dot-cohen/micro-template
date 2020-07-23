using System;
using System.Collections.Generic;

namespace Laso.Mediation
{
    public class CommandResponse : Response
    {
        public CommandResponse() { }

        public CommandResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null) : base(failures, exception)
        {
        }

        public static CommandResponse Succeeded() => new CommandResponse();
        public static CommandResponse<TResult> Succeeded<TResult>(TResult result) => new CommandResponse<TResult>(result: result);

        public static CommandResponse<TResult> Failed<TResult>(string message) => new CommandResponse<TResult>(new[] { new ValidationMessage(string.Empty, message) });
        public static CommandResponse<TResult> Failed<TResult>(string key, string message) => new CommandResponse<TResult>(new[] { new ValidationMessage(key, message) });
        public static CommandResponse<TResult> Failed<TResult>(ValidationMessage message) => new CommandResponse<TResult>(new[] {message});
        public static CommandResponse<TResult> Failed<TResult>(IEnumerable<ValidationMessage> messages) => new CommandResponse<TResult>(messages);
        public static CommandResponse<TResult> Failed<TResult>(Exception exception) => new CommandResponse<TResult>(exception: exception);
        public static CommandResponse<TResult> Failed<TResult>(Response response)
        {
            if (response.Success)
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return response.ToResponse<CommandResponse<TResult>>();
        }
    }

    public class CommandResponse<TResult> : CommandResponse
    {
        public CommandResponse() { }

        public CommandResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null, TResult result = default) : base(failures, exception)
        {
            Result = result;
        }
        public TResult Result { get; set; }
    }
}
