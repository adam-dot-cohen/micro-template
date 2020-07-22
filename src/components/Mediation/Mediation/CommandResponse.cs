using System;
using System.Collections.Generic;

namespace Laso.Mediation
{
    public class CommandResponse : Response
    {
        public static CommandResponse Succeeded() => new CommandResponse { IsValid = true };
        public static CommandResponse<TResult> Succeeded<TResult>(TResult result) => new CommandResponse<TResult> { IsValid = true, Result = result };

        public static CommandResponse<TResult> Failed<TResult>(string message) => new CommandResponse<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(string.Empty, message) } };
        public static CommandResponse<TResult> Failed<TResult>(string key, string message) => new CommandResponse<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(key, message) } };
        public static CommandResponse<TResult> Failed<TResult>(params ValidationMessage[] messages) => new CommandResponse<TResult> { IsValid = false, ValidationMessages = messages };
        public static CommandResponse<TResult> Failed<TResult>(Exception exception) => new CommandResponse<TResult> { IsValid = false, Exception = exception };
        public static CommandResponse<TResult> Failed<TResult>(Response response) => new CommandResponse<TResult> { IsValid = false, Exception = response.Exception, ValidationMessages = response.ValidationMessages };
        public static QueryResponse<TResult> Failed<TResult>(Response response)
        {
            if (response.Success())
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return new QueryResponse<TResult> {Exception = response.Exception, ValidationMessages = response.ValidationMessages};
        }
    }

    public class CommandResponse<TResult> : CommandResponse
    {
        public TResult Result { get; set; }
    }
}
