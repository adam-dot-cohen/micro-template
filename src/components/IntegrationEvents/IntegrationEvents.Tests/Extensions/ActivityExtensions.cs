using System.Diagnostics;

namespace Laso.IntegrationEvents.Tests.Extensions
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

        public static string GetTraceParent(this Activity activity)
        {
            return  $"00-{activity.TraceId.ToHexString()}-{activity.ParentSpanId.ToHexString()}-00";
        }
    }
}
