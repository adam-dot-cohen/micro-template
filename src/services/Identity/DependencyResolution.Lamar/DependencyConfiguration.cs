using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Laso.Identity.Infrastructure.Mediator.Pipeline;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Hosting;

namespace Laso.Identity.DependencyResolution.Lamar
{
    public class DependencyConfiguration
    {
        public void Configure(IHostBuilder builder)
        {
            var registry = new ServiceRegistry();
            ConfigureContainer(registry);

            builder
                .UseLamar(registry)
                // .ConfigureServices((_, services) => services
                    // We could be smarter about weather forecast service dependencies.
                    // .AddHttpClient()
                    // .AddGrpcClient<WeatherForecast.WeatherForecastClient>(o => o.Address = new Uri("https://localhost:5002"))
                // )
                ;
        }

        private static void ConfigureContainer(ServiceRegistry x)
        {
            x.Scan(scan =>
            {
                scan.Assembly("Laso.Identity.Infrastructure");
                scan.Assembly("Laso.Identity.Core");
                scan.WithDefaultConventions();

                // Mediator
                scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
            });

            ConfigureMediator(x);
        }

        private static ServiceRegistry ConfigureMediator(ServiceRegistry x)
        {
            //Pipeline gets executed in order
            x.For(typeof(IPipelineBehavior<,>)).Add(typeof(LoggingPipelineBehavior<,>));
            x.For(typeof(IPipelineBehavior<,>)).Add(typeof(ExceptionPipelineBehavior<,>));
            x.For(typeof(IPipelineBehavior<,>)).Add(typeof(ValidationPipelineBehavior<,>));
            x.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            x.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));

            x.For<IMediator>().Use<Mediator>().Scoped();
            x.For<ServiceFactory>().Use(ctx => ctx.GetInstance);

            return x;
        }
    }
}