using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.SignalR;
using Laso.AdminPortal.Web.Authentication;
using Laso.AdminPortal.Web.Configuration;
using Laso.AdminPortal.Web.Extensions;
using Laso.Hosting;
using Laso.Hosting.Health;
using Laso.IntegrationEvents;
using Laso.IO.Serialization;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using LasoAuthenticationOptions = Laso.AdminPortal.Infrastructure.Configuration.AuthenticationOptions;

namespace Laso.AdminPortal.Web
{
    // TODO: This class needs some cleanup -- in the least isolate configuration types in separate methods. [jay_mclain]
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            // Use claim types as we define them rather than mapping them to url namespaces
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                .Configure<ServicesOptions>(_configuration.GetSection(ServicesOptions.Section))
                .Configure<IdentityServiceOptions>(_configuration.GetSection(IdentityServiceOptions.Section))
                .Configure<LasoAuthenticationOptions>(_configuration.GetSection(LasoAuthenticationOptions.Section));
            IdentityModelEventSource.ShowPII = true;

            if (!_environment.IsDevelopment())
            {
                // Enable Application Insights telemetry collection.
                services.AddApplicationInsightsTelemetry();
            }

            services.AddSignalR();
            services.AddControllers();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IHostEnvironment>(_environment);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                // .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                // {
                //     options.AccessDeniedPath = "/Authorization/AccessDenied";
                // })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    var authOptions = _configuration.GetSection(LasoAuthenticationOptions.Section).Get<LasoAuthenticationOptions>();
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = authOptions.AuthorityUrl;
                    // RequireHttpsMetadata = false;
                    options.ClientId = authOptions.ClientId;
                    options.ClientSecret = authOptions.ClientSecret;
                    options.ResponseType = "code"; // Authorization Code flow, with PKCE (see below)
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("email");
                    options.Scope.Add("identity_api");
                    options.Scope.Add("provisioning_api");
                    options.SaveTokens = true;

                    // If API call, return 401
                    options.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        if (ctx.Response.StatusCode == StatusCodes.Status200OK && IsApiRequest(ctx.Request))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };
                    // TODO: What about 403??
                    // options.Events.OnAccessDenied = ctx => { };
                });

            services.AddHttpClient("IDPClient", (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<LasoAuthenticationOptions>>().CurrentValue;
                client.BaseAddress = new Uri(options.AuthorityUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
            // Disable authentication based on settings
            if (!IsAuthenticationEnabled())
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
            }

            services.AddHealthChecks()
                .AddCheck<EnvironmentHealthCheck>(nameof(EnvironmentHealthCheck));

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // AddLogging is an extension method that pipes into the ASP.NET Core service provider.  
            // You can peek it and implement accordingly if your use case is different, but this makes it easy for the common use cases. 
            // services.AddLogging(BuildLoggingConfiguration());

            AddListeners(services);
        }

        private static void AddListeners(IServiceCollection services)
        {
            var listenerCollection = new ListenerCollection();

            listenerCollection.AddSubscription<ProvisioningCompletedEvent>(subscriptionName: "SignalR");
            listenerCollection.AddSubscription<DataPipelineStatus>(subscriptionName: "SignalR");
            listenerCollection.AddSubscription<Core.IntegrationEvents.PartnerFilesReceivedEvent>(subscriptionName: "SignalR");

            MoveToDataRouterService(listenerCollection);

            services.AddHostedService(sp => listenerCollection.GetHostedService(sp));
        }

        private static void MoveToDataRouterService(ListenerCollection listenerCollection)
        {
            listenerCollection.AddSubscription<DataPipelineStatus>();
            listenerCollection.AddCloudSubscription<InputDataReceivedEventV1>("CustomerData");

            //repackage queued event from blob storage
            listenerCollection.AddReceiver<FileUploadedToEscrowEvent>(sp => async (@event, cancellationToken) =>
            {
                var eventPublisher = sp.GetRequiredService<IEventPublisher>();
                await eventPublisher.Publish(new InputDataReceivedEventV1
                {
                    Uri = @event.Data.Url,
                    ETag = @event.Data.ETag,
                    ContentType = @event.Data.ContentType,
                    ContentLength = @event.Data.ContentLength
                }, "CustomerData");
            }, sp => new EncodingSerializer(
                sp.GetRequiredService<IJsonSerializer>().With(x => x.SetOptions(new JsonSerializationOptions { PropertyNameCasingStyle = CasingStyle.Camel })),
                new Base64Encoding()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!_environment.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            // app.UseSerilogRequestLogging();
            app.ConfigureRequestLoggingOptions();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Don't define routes, will use attribute routing
                endpoints.MapControllers();

                endpoints.MapHub<NotificationsHub>("/hub/notifications");
                endpoints.MapHub<DataAnalysisHub>("/hub/dataanalysis");

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
            });

            // Require authentication
            if (IsAuthenticationEnabled())
            {
                app.Use(async (context, next) =>
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
                    }
                    else
                    {
                        await next();
                    }
                });
            }

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";
            
                if (_environment.IsDevelopment())
                {
                    // Configure the timeout to 5 minutes to avoid "The Angular CLI process did not
                    // start listening for requests within the timeout period of {0} seconds." 
                    spa.Options.StartupTimeout = TimeSpan.FromMinutes(5);
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        // private LoggingConfiguration BuildLoggingConfiguration()
        // {
        //     var loggingSettings = new LoggingSettings();
        //     _configuration.GetSection("Laso:Logging:Common").Bind(loggingSettings);
        //
        //     var seqSettings = new SeqSettings();
        //     _configuration.GetSection("Laso:Logging:Seq").Bind(seqSettings);
        //
        //     var logglySettings = new LogglySettings();
        //     _configuration.GetSection("Laso:Logging:Loggly").Bind(logglySettings);
        //
        //     return  new LoggingConfigurationBuilder()
        //         .BindTo(new SeqSinkBinder(seqSettings))
        //         .BindTo(new LogglySinkBinder(loggingSettings, logglySettings))
        //         .Build(x => x.Enrich.ForLaso(loggingSettings));
        // }

        private bool IsAuthenticationEnabled()
        {
            return _configuration.GetSection(LasoAuthenticationOptions.Section)
                .Get<LasoAuthenticationOptions>()?.Enabled ?? true;
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            return
                string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) 
                || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal)
                || request.Path.StartsWithSegments("/api");
        }
    }
}
