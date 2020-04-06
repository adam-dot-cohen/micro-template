using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.Web.Extensions
{
    public static class DependencyResolutionExtensions
    {
        public static IHostBuilder ConfigureCustomDependencyResolution(this IHostBuilder builder, IConfiguration configuration)
        {
            var configurationTypeName = configuration["DependencyResolution:ConfigurationType"];
            var configurationType = Type.GetType(configurationTypeName, true);
            var configurationClass = Activator.CreateInstance(configurationType);
            var configureMethod = configurationType.GetMethod("Configure");

            if (configureMethod == null)
                throw new InvalidOperationException("DependencyResolution configuration method not found.");

            configureMethod.Invoke(configurationClass, new object[] { builder });

            return builder;
        }
    }
}
