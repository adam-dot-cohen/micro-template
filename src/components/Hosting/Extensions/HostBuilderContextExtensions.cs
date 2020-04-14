using Microsoft.Extensions.Hosting;

namespace Laso.Hosting.Extensions
{
    public static class HostBuilderContextExtensions
    {
        public static bool IsTest(this HostBuilderContext context)
        {
            return !string.IsNullOrEmpty(context.Configuration["testEnvironment"]);
        }
    }
}
