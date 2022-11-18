using Infrastructure.Logging.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Infrastructure.Logging.Extensions
{
    public static class WebLoggingExtensions
    {

        public static void ConfigureRequestLoggingOptions(this IApplicationBuilder app)
        {

            app.UseSerilogRequestLogging(options =>
            {
                // Customize the message template
                options.MessageTemplate = "Handled {RequestPath}";

                // Emit debug-level events instead of the defaults
                options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("ReferrerUrl", httpContext.Request.Headers["Referer"].ToString());
                };
            });
        }

        public static void AddLogging(this IServiceCollection services, LoggingConfiguration lc)
        {
            services.AddSingleton(lc);
            services.AddTransient<ILogService,LogService>();
        }
    }
}