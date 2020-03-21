using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Mediator
{
    /// <summary>
    ///     Intermediary for request sent to or from the broker / mediator
    ///     <see href="http://en.wikipedia.org/wiki/Mediator_pattern"></see>
    ///     <see href="http://en.wikipedia.org/wiki/Command%E2%80%93query_separation"></see>
    ///     <see href="http://martinfowler.com/bliki/CommandQuerySeparation.html"></see>
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Commands are system operations that alter state of the system. They are invoked by a message called a command. They are handled by a Command Handler which will always return a CommandResponse, regardless of the success for failure of executing (handling) the command.
        /// A command can fail for two sets of reasons. 
        /// 1. The command is not Valid, resulting in Validation Messages being returned with a Failed Command Response.
        /// 2. An exception can occur during the execution of the handler which will return a Failed Command Response that contains the Exception that occurred.
        /// Commands: Change the state of a system but do not return a value. 
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A CommandResponse containing the result of the command</returns>
        Task<CommandResponse> Command(ICommand command, CancellationToken cancellationToken);
        Task<CommandResponse<TResult>> Command<TResult>(ICommand<TResult> command, CancellationToken cancellationToken);

        /// <summary>
        /// Queries: Return a result and do not change the observable state of the system (are free of side effects).
        /// </summary>
        /// <typeparam name="TResult">The result of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A QueryResponse that contains the response information of the query.</returns>
        Task<QueryResponse<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    }
}
