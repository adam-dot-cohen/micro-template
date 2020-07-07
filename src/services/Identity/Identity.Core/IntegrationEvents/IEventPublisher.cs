using System;
using System.Threading.Tasks;

namespace Laso.Identity.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IIntegrationEvent;
    }

    public interface IIntegrationEvent { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EnvelopePropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
