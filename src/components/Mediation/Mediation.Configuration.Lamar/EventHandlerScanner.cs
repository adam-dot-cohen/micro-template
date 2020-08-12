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
                if (!type.Closes(typeof(IEventHandler<>), out var args))
                    continue;

                services.Add(new ServiceDescriptor(typeof(INotificationHandler<>).MakeGenericType(args[0]), typeof(EventPipeline<,>).MakeGenericType(args[0], type), ServiceLifetime.Transient));
            }
        }
    }
}