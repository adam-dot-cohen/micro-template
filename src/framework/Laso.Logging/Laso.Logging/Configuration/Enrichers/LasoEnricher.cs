using System;
using System.Collections.Generic;
using System.Threading;
using Laso.Logging.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Laso.Logging.Configuration.Enrichers
{
    public class LasoEnricher: ILogEventEnricher
    {
        public static LoggingSettings CommonSettings { get; set; }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var dictionary = new Dictionary<string, object>
            {
                { "Environment", CommonSettings.Environment },
                { "Application", CommonSettings.Application},
                { "Version", CommonSettings.Version},
                { "MachineName", Environment.MachineName },
                { "AppDomain", AppDomain.CurrentDomain.Id },
                { "ThreadId", Environment.CurrentManagedThreadId },
                { "CurrentPrincipal", Thread.CurrentPrincipal?.Identity?.Name.EmptyStringToNull() },
                { "timestamp", logEvent.Timestamp.UtcDateTime.ToString("O") },
                { "Tenant",  CommonSettings.TenantName }
            };
            foreach (var entry in dictionary)
            {
                if (entry.Value != null)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(entry.Key, entry.Value));
                }
            }
        }
    }
}
