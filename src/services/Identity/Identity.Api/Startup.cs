using System.IO;
using Laso.Identity.Api.Configuration;
using Laso.Identity.Api.Services;
using Laso.Identity.Core.Messaging;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Infrastructure.Eventing;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Laso.Identity.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddTestUsers(TestUsers.Users());

            // in-memory, code config
            builder.AddInMemoryIdentityResources(Config.GetResources());
            builder.AddInMemoryApiResources(Config.GetApis());
            builder.AddInMemoryClients(Config.GetClients(_configuration.GetSection("AuthClients")["AdminPortalClientUrl"]));

            // or in-memory, json config
            //builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            //builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            // builder.AddInMemoryClients(_configuration.GetSection("IdentityServer:Clients"));

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddSigningCredential(Certificate.Get());

            // Enable Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            // services.AddAuthentication();
            // services.AddAuthorization();
            services.AddMvc();
           

            services.AddTransient<ITableStorageContext>(x => new AzureTableStorageContext(
                    _configuration.GetConnectionString("IdentityTableStorage"),
                "identity",
                    new ISaveChangesDecorator[0],
                    new IPropertyColumnMapper[]
            {
                new EnumPropertyColumnMapper(),
                new DelimitedPropertyColumnMapper(),
                new DefaultPropertyColumnMapper()
            }));
            services.AddTransient<ITableStorageService, AzureTableStorageService>();
            services.AddTransient<IEventPublisher>(x => new AzureServiceBusEventPublisher(_configuration.GetConnectionString("EventServiceBus")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapGrpcService<PartnersServiceV1>();
                
                // endpoints.MapGet("/", async context =>
                // {
                    // await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                // });
            });
        }
    }

    
}
