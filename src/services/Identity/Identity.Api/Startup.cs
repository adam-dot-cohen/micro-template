using IdentityServer4.AccessTokenValidation;
using Laso.Identity.Api.Configuration;
using Laso.Identity.Api.Services;
using Laso.Identity.Core.Messaging;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Infrastructure.Eventing;
using Laso.Identity.Infrastructure.Persistence.Azure;
using Laso.Identity.Infrastructure.Persistence.Azure.PropertyColumnMappers;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using LasoAuthenticationOptions = Laso.Identity.Api.Configuration.AuthenticationOptions;

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
            IdentityModelEventSource.ShowPII = true;

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

            // Don't set default authentication scheme here since 'idsrv' is set to the default in
            // AddIdentityServer() call above.
            // Here we are adding Bearer (JWT) authentication to be used for API calls only
            services.AddAuthentication()
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,options =>
                {
                    var authOptions = _configuration.GetSection(LasoAuthenticationOptions.Section).Get<LasoAuthenticationOptions>();
                    options.Authority = authOptions.AuthorityUrl;
                    options.ApiName = authOptions.ClientId;
                    options.ApiSecret = authOptions.ClientSecret;
                });

            // Disable authentication based on settings
            if (!IsAuthenticationEnabled())
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
            }

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
                    new ComponentPropertyColumnMapper(new IPropertyColumnMapper[]
                    {
                        new EnumPropertyColumnMapper(),
                        new DelimitedPropertyColumnMapper(),
                        new DefaultPropertyColumnMapper()
                    }),
                    new DefaultPropertyColumnMapper()
                }));
            services.AddTransient<ITableStorageService, AzureTableStorageService>();
            services.AddTransient<IEventPublisher>(x => new AzureServiceBusEventPublisher(new AzureTopicProvider(_configuration.GetConnectionString("EventServiceBus"), _configuration["Laso:ServiceBus:TopicNameFormat"])));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureRequestLoggingOptions();

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseRouting();

            // Add gRPC-Web middleware after routing and before endpoints
            app.UseGrpcWeb();

            if (IsAuthenticationEnabled())
            {
                app.UseAuthentication();
            }
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapGrpcService<PartnersServiceV1>().EnableGrpcWeb();
                
                // endpoints.MapGet("/", async context =>
                // {
                    // await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                // });
            });
        }

        private bool IsAuthenticationEnabled()
        {
            return _configuration.GetSection(LasoAuthenticationOptions.Section).Get<LasoAuthenticationOptions>().Enabled;
        }

    }
}
