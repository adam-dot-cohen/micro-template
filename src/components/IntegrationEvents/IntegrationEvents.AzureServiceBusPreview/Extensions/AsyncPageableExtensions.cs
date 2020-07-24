using System.Collections.Generic;
using Azure;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview.Extensions
{
    internal static class AsyncPageableExtensions
    {
        public static async IAsyncEnumerable<T> ToEnumerable<T>(this AsyncPageable<T> asyncPageable)
        {
            await foreach (var item in asyncPageable)
                yield return item;
        }
    }
}
