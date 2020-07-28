using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Laso.Hosting
{
    /// <summary>
    /// Hosts a collection of listeners in a single hosted service.
    /// This gets around <see cref="IServiceCollection"/>'s limitation of only allowing one <see cref="IHostedService"/> of a given Type.
    /// This is a problem, for instance, when subscribing to a single topic with multiple subscriptions (e.g. with filtering)
    /// using AzureServiceBusSubscriptionEventListener<TEvent>.
    /// </summary>
    public class ListenerCollection
    {
        private readonly ICollection<Func<IServiceProvider, Func<CancellationToken, Task>>> _listeners = new List<Func<IServiceProvider, Func<CancellationToken, Task>>>();

        public void Add(Func<IServiceProvider, Func<CancellationToken, Task>> listener)
        {
            _listeners.Add(listener);
        }

        public ListenerCollectionHostedService GetHostedService(IServiceProvider serviceProvider)
        {
            return new ListenerCollectionHostedService(_listeners.Select(x => x(serviceProvider)).ToList());
        }
    }

    public class ListenerCollectionHostedService : BackgroundService
    {
        private readonly ICollection<Func<CancellationToken, Task>> _listeners;

        internal ListenerCollectionHostedService(ICollection<Func<CancellationToken, Task>> listeners)
        {
            _listeners = listeners;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.WhenAll(_listeners.Select(x => x(stoppingToken)));
        }
    }
}
