using System.Collections.Generic;
using Azure;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview.Extensions
{
    internal static class AsyncPageableExtensions
    {
        public static IAsyncEnumerable<T> ToEnumerable<T>(this AsyncPageable<T> asyncPageable)
        {
            return asyncPageable;
        }
    }
}
