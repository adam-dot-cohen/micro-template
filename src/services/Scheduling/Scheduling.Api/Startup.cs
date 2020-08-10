using System;
using System.Threading.Tasks;
using Lamar;
using Laso.Hosting;
using Laso.Hosting.Health;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.CloudEvents;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Mediation.Configuration.Lamar;
using Laso.Scheduling.Api.Extensions;
using Laso.Scheduling.Api.IntegrationEvents.CustomerData;
using Laso.Scheduling.Api.Services;
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
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureContainer(ServiceRegistry services)
        {
            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry();
            }

            services.Scan(scan =>
            {
                scan.Assembly("Laso.Scheduling.Core");
                scan.Assembly("Laso.Scheduling.Infrastructure");
                scan.WithDefaultConventions();

                scan.AddMediatorHandlers();
            });

            services.AddMediator().WithDefaultMediatorBehaviors();

            services.AddHealthChecks();

            services.AddGrpc();

            services.AddTransient<IMessageBuilder>(sp => new CloudEventMessageBuilder(new NewtonsoftSerializer(), new Uri("service://scheduling")));

            services.AddTransient<IEventPublisher>(sp => new AzureServiceBusEventPublisher(
                GetTopicProvider(_configuration),
                sp.GetRequiredService<IMessageBuilder>()));

            var listenerCollection = new ListenerCollection();

            listenerCollection.AddSubscription<InputBatchAcceptedEventV1>(
                sp => GetTopicProvider(_configuration),
                "CustomerData",
                sp => (@event, cancellationToken) => Task.CompletedTask /*TODO*/);

            services.AddHostedService(sp => listenerCollection.GetHostedService(sp));
        }

        private static AzureServiceBusTopicProvider GetTopicProvider(IConfiguration configuration)
        {
            return new AzureServiceBusTopicProvider(
                configuration.GetSection("Services:Scheduling:IntegrationEventHub").Get<AzureServiceBusConfiguration>(),
                configuration["Services:Scheduling:IntegrationEventHub:ConnectionString"]);
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
