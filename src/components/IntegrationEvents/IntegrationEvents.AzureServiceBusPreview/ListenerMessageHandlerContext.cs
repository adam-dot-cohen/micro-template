using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public class ListenerMessageHandlerContext<T> : IDisposable
    {
        private readonly IDisposable _scope;
        private readonly Activity _activity;

        public ListenerMessageHandlerContext(Func<T, CancellationToken, Task> eventHandler, IDisposable scope = null, string traceParent = null, string traceState = null)
        {
            EventHandler = eventHandler;
            TraceParent = traceParent;
            TraceState = traceState;

            _scope = scope;

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            _activity = new Activity(typeof(T).Name + "Handler");
            _activity.SetParentId(traceParent);
            _activity.TraceStateString = traceState;
            _activity.Start();
        }

        public Func<T, CancellationToken, Task> EventHandler { get; }

        public string TraceParent { get; }
        public string TraceState { get; }

        public void Dispose()
        {
            _activity.Stop();
            _scope?.Dispose();
        }
    }
}