namespace Laso.AdminPortal.Core.Mediator
{
    public class QueryResponse : Response
    {
        public static QueryResponse<TResult> Succeeded<TResult>(TResult result)
        {
            return new QueryResponse<TResult>
            {
                IsValid = true,
                Result = result 
            };
        }
    }

    public class QueryResponse<TResult> : QueryResponse
    {
        public TResult Result { get; set; }
    }
}
