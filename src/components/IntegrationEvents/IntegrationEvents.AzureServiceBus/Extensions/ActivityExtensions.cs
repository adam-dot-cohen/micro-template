using System.Diagnostics;

namespace Laso.IntegrationEvents.AzureServiceBus.Extensions
{
    internal static class ActivityExtensions
    {
        public static void SetTraceParent(this Activity activity, string traceParent = null)
        {
            if (traceParent == null)
            {
                activity.SetParentId(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom());
                return;
            }

            var tokens = traceParent.Split('-');

            if (tokens.Length != 4)
            {
                activity.SetParentId(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom());
                return;
            }

            activity.SetParentId(ActivityTraceId.CreateFromString(tokens[1].ToCharArray()), ActivitySpanId.CreateFromString(tokens[2].ToCharArray()));
        }
    }
}
