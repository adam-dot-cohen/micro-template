using System;
using Lamar;
using Laso.Hosting;
using Laso.Hosting.Health;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IO.Serialization.Newtonsoft;
using Infrastructure.Mediation.Configuration.Lamar;
using Laso.Scheduling.Api.Extensions;
using Laso.Scheduling.Api.Services;
using Laso.Scheduling.Core.IntegrationEvents.Subscribe.CustomerData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Laso.Scheduling.Api
{
    public class Startup
    {
        private readonly IHostEnvironment _environment;

        public Startup(IHostEnvironment environment)
        {
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureContainer(ServiceRegistry services)
        {
            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                //services.AddApplicationInsightsTelemetry();
            }

            services.Scan(scan =>
            {
                scan.Assembly("Laso.Scheduling.Core");
                scan.Assembly("Laso.Scheduling.Infrastructure");
                scan.WithDefaultConventions();

                scan.AddMediatorHandlers();
            });

            services.AddMediator().WithDefaultMediatorBehaviors();

            services.AddHealthChecks()
                .AddCheck<EnvironmentHealthCheck>(nameof(EnvironmentHealthCheck));

            services.AddGrpc();

            services.AddTransient<IMessageBuilder>(sp => new CloudEventMessageBuilder(sp.GetRequiredService<NewtonsoftSerializer>(), new Uri("app://services/scheduling")));

            services.AddTransient<IEventPublisher>(sp => new AzureServiceBusEventPublisher(
                sp.GetRequiredService<AzureServiceBusTopicProvider>(),
                sp.GetRequiredService<IMessageBuilder>()));

            services.ForConcreteType<AzureServiceBusTopicProvider>().Configure
                .Ctor<AzureServiceBusConfiguration>().Is(c => c.GetInstance<IConfiguration>().GetSection("Services:Scheduling:IntegrationEventHub").Get<AzureServiceBusConfiguration>())
                .Ctor<string>().Is(c => c.GetInstance<IConfiguration>()["Services:Scheduling:IntegrationEventHub:ConnectionString"]!);

            AddListeners(services);
        }

        private static void AddListeners(IServiceCollection services)
        {
            var listenerCollection = new ListenerCollection();

            listenerCollection.AddSubscription<InputBatchAcceptedEventV1>("CustomerData");

            services.AddHostedService(sp => listenerCollection.GetHostedService(sp));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapHealthChecks(
                    "/health",
                    new HealthCheckOptions
                    {
                        AllowCachingResponses = false,
                        ResponseWriter = JsonHealthReportResponseWriter.WriteResponse,
                        ResultStatusCodes =
                        {
                            [HealthStatus.Healthy] = StatusCodes.Status200OK,
                            [HealthStatus.Degraded] = StatusCodes.Status200OK,
                            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                        }
                    });

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
