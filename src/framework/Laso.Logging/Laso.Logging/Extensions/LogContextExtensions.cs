using System;
using System.Collections.Generic;
using System.Text;
using Laso.Logging.Collections;

namespace Laso.Logging.Extensions
{
    // public static class LogContextExtensions
    // {
    //     public static ILogContext ToStaticLogContext(this ILogContext logContext)
    //     {
    //         logContext = logContext ?? new LogContext();
    //         var currentBatches = BatchContext.GetCurrent();
    //         var (callStack, topCallers) = GetCallStackInfo();
    //         return new StaticLogContext
    //         {
    //             CorrelationId = logContext.CorrelationId,
    //             ScopeId = logContext.ScopeId,
    //             IsNestedScope = logContext.IsNestedScope,
    //             AssociatedEntities = logContext.AssociatedEntities.ToArray(),
    //             Properties = new MultiValueDictionary<string, string>(logContext.Properties),
    //             Batches = currentBatches.Select(x => x.Id).ToArray(),
    //             CallStack = callStack,
    //             TopCallers = topCallers
    //         };
    //     }
    //
    //     private static (string callStack, string topCallers) GetCallStackInfo()
    //     {
    //         string topCallers = null;
    //         string callStack = null;
    //         var executionStack = HandlerExecutionContextStack.GetCurrent() ?? new IHandlerExecutionContext[0];
    //         if (executionStack.Count > 0)
    //         {
    //             // Only take the last 2 callers to support filtering in Loggly. Call stack can get too
    //             // big to show up in Loggly's filter fields.
    //             topCallers = string.Join(" > ", executionStack.Skip(Math.Max(0, executionStack.Count - 2)).Take(2).Select(x => x.HandlerType.Name));
    //
    //             if (executionStack.Count > 2)
    //             {
    //                 // To save space, only set the call stack if it is deeper than the 2 levels already captured in topCallers
    //                 callStack = string.Join(" > ", executionStack.Select(x => x.HandlerType.Name));
    //             }
    //         }
    //
    //         return (callStack, topCallers);
    //     }
    //
    //     private class StaticLogContext : ILogContext
    //     {
    //         public Guid CorrelationId { get; set; }
    //         public Guid ScopeId { get; set; }
    //         public bool IsNestedScope { get; set; }
    //         public IEnumerable<Guid> Batches { get; set; }
    //         public string TopCallers { get; set; }
    //         public string CallStack { get; set; }
    //
    //         public IEnumerable<AssociatedEntity> AssociatedEntities { get; set; }
    //
    //         public void AddAssociatedEntity(AssociatedEntity associatedEntity)
    //         {
    //             throw new NotImplementedException("StaticLogContext is intended to never be changed");
    //         }
    //
    //         public void AddAssociatedEntities(IEnumerable<AssociatedEntity> associatedEntities)
    //         {
    //             throw new NotImplementedException("StaticLogContext is intended to never be changed");
    //         }
    //
    //         public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Properties { get; set; }
    //         public void AddProperty(string key, string value)
    //         {
    //             throw new NotImplementedException("StaticLogContext is intended to never be changed");
    //         }
    //     }
    // }
}
