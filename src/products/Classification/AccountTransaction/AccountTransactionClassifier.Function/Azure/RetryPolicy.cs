using System;
using Polly;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public class RetryPolicy : IRetryPolicy
    {
        /// <inheritdoc cref="IRetryPolicy.RetryOnException{TException,TResult}"/>
        public TResult RetryOnException<TException, TResult>(
            int retryCount,
            Func<int, TimeSpan> sleepDurationProvider,
            Func<TResult> action)
            where TException : Exception
        {
            return Policy.Handle<TException>()
                .WaitAndRetry(retryCount, sleepDurationProvider)
                .Execute(action);
        }

        /// <inheritdoc cref="IRetryPolicy.RetryOnResult{TResult}"/>
        public TResult RetryOnResult<TResult>(
            int retryCount,
            Func<TResult, bool> resultPredicate,
            Func<int, TimeSpan> sleepDurationProvider,
            Func<TResult> action)
        {
            return Policy.HandleResult(resultPredicate)
                .WaitAndRetry(retryCount, sleepDurationProvider)
                .Execute(action);
        }
    }
}
