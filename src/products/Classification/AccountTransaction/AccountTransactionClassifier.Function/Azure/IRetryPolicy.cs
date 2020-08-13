using System;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public interface IRetryPolicy
    {
        /// <summary>
        /// Retry the given action retryCount number of times until it succeeds while the exception
        /// being thrown is of type TException.
        /// </summary>
        /// <typeparam name="TException">The type of exception that triggers a retry</typeparam>
        /// <typeparam name="TResult">The result type of the action</typeparam>
        /// <param name="retryCount">The number of times to retry after the first failure</param>
        /// <param name="sleepDurationProvider">A func to compute the amount of time to delay between retries</param>
        /// <param name="action">The action to perform/monitor</param>
        /// <returns>The result of the successful action or throws an exception</returns>
        TResult RetryOnException<TException, TResult>(
            int retryCount,
            Func<int, TimeSpan> sleepDurationProvider,
            Func<TResult> action)
            where TException : Exception;

        /// <summary>
        /// Retry the given action retryCount number of times until it succeeds while the result
        /// being returned matches the given resultPredicate.
        /// </summary>
        /// <typeparam name="TResult">The result type of the action</typeparam>
        /// <param name="retryCount">The number of times to retry after the first failure</param>
        /// <param name="resultPredicate">Predicate to determine whether the action should be retried</param>
        /// <param name="sleepDurationProvider">A func to compute the amount of time to delay between retries</param>
        /// <param name="action">The action to perform/monitor</param>
        /// <returns>The result of the successful action</returns>
        TResult RetryOnResult<TResult>(
            int retryCount,
            Func<TResult, bool> resultPredicate,
            Func<int, TimeSpan> sleepDurationProvider,
            Func<TResult> action);
    }

    public static class DelayCalculator
    {
        /// <summary>
        /// Calculate an exponential back-off delay up to a max number of seconds.
        /// The delay series starts as 0, 1, 3, 7, 15.
        /// </summary>
        public static int ExponentialDelay(int retryAttempt, int maxDelayInSeconds = 1024)
        {
            var delayInSeconds = (1d / 2d) * (Math.Pow(2d, retryAttempt) - 1d);

            return maxDelayInSeconds < delayInSeconds
                ? maxDelayInSeconds
                : (int)delayInSeconds;
        }
    }
}
