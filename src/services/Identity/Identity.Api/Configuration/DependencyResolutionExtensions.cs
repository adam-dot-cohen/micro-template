using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Identity.Api.Configuration
{
    public static class DependencyResolutionExtensions
    {
        public static IHostBuilder UseCustomDependencyResolution(this IHostBuilder builder, IConfiguration configuration)
        {
            var configurationTypeName = configuration["DependencyResolution:ConfigurationType"];
            var configurationType = Type.GetType(configurationTypeName, true);
            var configurationClass = Activator.CreateInstance(configurationType);
            var configureMethod = configurationType.GetMethod("Configure");
            configureMethod.Invoke(configurationClass, new object[] { builder });

            return builder;
        }
    }
}