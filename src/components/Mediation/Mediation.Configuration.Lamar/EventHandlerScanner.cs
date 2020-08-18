using BaselineTypeDiscovery;
using Lamar;
using Lamar.Scanning.Conventions;
using Laso.Mediation.Behaviors;
using Laso.Mediation.Configuration.Lamar.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Laso.Mediation.Configuration.Lamar
{
    public class EventHandlerScanner : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.AllTypes())
            {
                if (type.Closes(typeof(IEventHandler<>), out var args))
                {
                    foreach (var arg in args)
                    {
                        var serviceType = typeof(INotificationHandler<>).MakeGenericType(arg[0]);
                        var implementationType = typeof(EventPipeline<,>).MakeGenericType(arg[0], type);

                        services.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
                    }
                }
            }
        }
    }
}