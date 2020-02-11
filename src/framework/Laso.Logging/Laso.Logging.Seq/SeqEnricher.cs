using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Laso.Logging.Seq
{
   public class SeqEnricher : ILogEventEnricher
    {
        private readonly string _environment;
        private readonly string _application;
        private readonly string _version;
        private readonly string _tenantName;
        private readonly IHttpContextAccessor _accessor;

        public SeqEnricher(IHttpContextAccessor accessor,string environment,string application, string version,string tenantName)
        {
            _environment = environment;
            _application = application;
            _version = version;
            _tenantName = tenantName;
            _accessor = accessor;
        }



        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var dictionary = new Dictionary<string, object>
            {
                { "Environment", _environment },
                { "Application", _application},
                { "Version", _version},
                { "MachineName", Environment.MachineName },
                { "AppDomain", AppDomain.CurrentDomain.Id },
                { "ThreadId", Environment.CurrentManagedThreadId },
                { "CurrentPrincipal", Thread.CurrentPrincipal?.Identity?.Name.EmptyStringToNull() },
                { "ClientIP", GetRemoteIpAddress(_accessor?.HttpContext)?.ToString().EmptyStringToNull() },
                { "RawUrl", GetRawUrl().EmptyStringToNull() },
                { "ReferrerUrl", GetReferrerUrl().EmptyStringToNull() },
                { "timestamp", logEvent.Timestamp.UtcDateTime.ToString("O") },
                { "Tenant",  _tenantName }
            };
            foreach (var entry in dictionary)
            {
                if (entry.Value != null)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(entry.Key, entry.Value));
                }
            }
        }

        public static IPAddress GetRemoteIpAddress(HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string header = (context?.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context?.Request?.Headers["X-Forwarded-For"].FirstOrDefault());
                if (header == null) return null;

                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }
            return context.Connection.RemoteIpAddress;
        }

        private string GetReferrerUrl()
        {
            return _accessor?.HttpContext?.Request?.GetTypedHeaders()?.Referer?.PathAndQuery;
        }

        private string GetRawUrl()
        {
            return _accessor?.HttpContext?.Request.GetDisplayUrl();
        }
    }
}
