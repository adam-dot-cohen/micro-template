using IdentityServer4.AccessTokenValidation;
using Lamar;
using Laso.Hosting.Health;
using Laso.Identity.Api.Configuration;
using Laso.Identity.Api.Services;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using LasoAuthenticationOptions = Laso.Identity.Api.Configuration.AuthenticationOptions;

namespace Laso.Identity.Api
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
            builder.AddInMemoryIdentityResources(IdentityProviderConfig.GetResources());
            builder.AddInMemoryApiResources(IdentityProviderConfig.GetApis());
            builder.AddInMemoryClients(IdentityProviderConfig.GetClients(_configuration.GetSection("AuthClients")["AdminPortalClientUrl"]));

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

            services.AddHealthChecks();

            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry();
            }

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
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
                app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapGrpcService<PartnersServiceV1>().EnableGrpcWeb();
                
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

                // REVIEW: Reason this was disabled? We *think* because it might have been conflicting with
                // identity UI web app, but not 100% sure. Some of the Application Insights health probes
                // look like they are hitting the root and failing. I don't want to re-enable it quite yet,
                // but we should review if we can re-enable this or update AI probe URLs. [jay_mclain]
                // endpoints.MapGet("/", async context =>
                // {
                    // await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                // });
            });
        }

        private bool IsAuthenticationEnabled()
        {
            var authenticationSection = _configuration.GetSection(LasoAuthenticationOptions.Section);
            var authenticationOptions = authenticationSection.Get<LasoAuthenticationOptions>();

            return authenticationOptions.Enabled;
        }
    }
}
