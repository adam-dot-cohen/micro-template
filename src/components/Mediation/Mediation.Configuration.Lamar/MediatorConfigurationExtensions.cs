using Infrastructure.Mediation.Behaviors;
using Lamar;
using Lamar.Scanning.Conventions;
using MediatR;
using MediatR.Pipeline;

namespace Infrastructure.Mediation.Configuration.Lamar
{
    public static class MediatorConfigurationExtensions
    {
        public static IAssemblyScanner AddMediatorHandlers(this IAssemblyScanner scanner)
        {
            scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
            scanner.ConnectImplementationsToTypesClosing(typeof(IStreamRequestHandler<,>));
            scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
            scanner.ConnectImplementationsToTypesClosing(typeof(IRequestPreProcessor<>));
            scanner.ConnectImplementationsToTypesClosing(typeof(IRequestPostProcessor<,>));
            scanner.With(new EventHandlerScanner());
            return scanner;
        }

        public static ServiceRegistry AddMediator(this ServiceRegistry _)
        {
            _.For<IMediator>().Use<Mediator>();
            _.For<ServiceFactory>().Use(ctx => ctx.GetInstance);
            return _;
        }

        public static ServiceRegistry WithDefaultMediatorBehaviors(this ServiceRegistry _)
        {
            //Pipeline gets executed in order
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(PerfLoggingPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ErrorLoggingPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ExceptionPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(ValidationPipelineBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            _.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));

            //Event pipeline
            _.For(typeof(IEventPipelineBehavior<>)).Add(typeof(PerfLoggingEventPipelineBehavior<>));
            _.For(typeof(IEventPipelineBehavior<>)).Add(typeof(ErrorLoggingEventPipelineBehavior<>));
            _.For(typeof(IEventPipelineBehavior<>)).Add(typeof(ExceptionEventPipelineBehavior<>));
            return _;
        }
    }
}