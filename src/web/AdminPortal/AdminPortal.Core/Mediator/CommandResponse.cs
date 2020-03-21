namespace Laso.AdminPortal.Core.Mediator
{
    public class CommandResponse : Response
    {
        public static CommandResponse Succeeded()
        {
            return new CommandResponse
            {
                IsValid = true
            };
        }

        public static CommandResponse<TResult> Succeeded<TResult>(TResult result)
        {
            return new CommandResponse<TResult>
            {
                IsValid = true,
                Result = result
            };
        }
    }

    public class CommandResponse<TResult> : CommandResponse
    {
        public TResult Result { get; set; }
    }
}
