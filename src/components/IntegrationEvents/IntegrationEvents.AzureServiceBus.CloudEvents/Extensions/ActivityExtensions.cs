using System.Diagnostics;

namespace Laso.IntegrationEvents.AzureServiceBus.CloudEvents.Extensions
{
    internal static class ActivityExtensions
    {
        public static string GetTraceParent(this Activity activity)
        {
            if (activity == null)
                return null;

            return  $"00-{activity.TraceId.ToHexString()}-{activity.ParentSpanId.ToHexString()}-00";
        }
    }
}
