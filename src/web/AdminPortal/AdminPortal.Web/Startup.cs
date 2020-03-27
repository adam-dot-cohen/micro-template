using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.AdminPortal.Infrastructure.IntegrationEvents;
using Laso.AdminPortal.Web.Authentication;
using Laso.AdminPortal.Web.Configuration;
using Laso.AdminPortal.Web.Hubs;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                .Configure<AzureStorageQueueOptions>(_configuration.GetSection(AzureStorageQueueOptions.Section))
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

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddTransient<IEventPublisher>(ctx => new AzureServiceBusEventPublisher(GetTopicProvider(_configuration)));

            // AddLogging is an extension method that pipes into the ASP.NET Core service provider.  
            // You can peek it and implement accordingly if your use case is different, but this makes it easy for the common use cases. 
            // services.AddLogging(BuildLoggingConfiguration());

            var eventListeners = new EventListenerCollection();

            AddSubscriptionListener<ProvisioningCompletedEvent>(eventListeners, _configuration, sp => async (@event, cancellationToken) =>
            {
                var hubContext = sp.GetService<IHubContext<NotificationsHub>>();
                await hubContext.Clients.All.SendAsync("Notify", "Partner provisioning complete!", cancellationToken: cancellationToken);
            });

            AddSubscriptionListener<DataPipelineStatus>(eventListeners, _configuration, sp =>
                async (@event, cancellationToken) =>
                {
                    var hubContext = sp.GetService<IHubContext<DataAnalysisHub>>();
                    var status = new AnalysisStatusViewModel
                    {
                        Timestamp = @event.Timestamp,
                        DataCategory = @event.Body?.Document?.DataCategory,
                        Status = @event.Stage ?? @event.EventType
                    };
                    await hubContext.Clients.All.SendAsync("Updated", status, cancellationToken: cancellationToken);
                }, "SignalR");

            //TODO: move to monitoring service
            {
                AddSubscriptionListener<DataPipelineStatus>(eventListeners, _configuration, sp => async (@event, cancellationToken) =>
                {
                    var mediator = sp.GetRequiredService<IMediator>();
                    await mediator.Command(new UpdatePipelineRunAddStatusEventCommand { Event = @event }, cancellationToken);
                }, "DataPipelineStatus", x => x.EventType != "DataAccepted");

                AddSubscriptionListener<DataPipelineStatus>(eventListeners, _configuration, sp => async (@event, cancellationToken) =>
                {
                    var mediator = sp.GetRequiredService<IMediator>();
                    await mediator.Command(new UpdateFileBatchToAcceptedCommand { Event = @event }, cancellationToken);
                }, "DataAccepted", x => x.EventType == "DataAccepted");

                AddFileUploadedToEscrowListenerHostedService(eventListeners);
            }

            services.AddHostedService(sp => eventListeners.GetHostedService(sp));
        }

        private void AddFileUploadedToEscrowListenerHostedService(EventListenerCollection eventListeners)
        {
            // messages from event grid a re base64 encoded 
            static FileUploadedToEscrowEvent DeserializeMessage(string messageText)
            {
                var messageBytes = Convert.FromBase64String(messageText);
                var messageBody = Encoding.UTF8.GetString(messageBytes);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = JsonSerializer.Deserialize<FileUploadedToEscrowEvent>(messageBody, options);

                return message;
            }

            eventListeners.Add(sp =>
                new AzureStorageQueueEventListener<FileUploadedToEscrowEvent>(
                    new AzureStorageQueueProvider(
                        // NOTE: YES, storage queues are using the table storage connection string!
                        // For now we need to reuse the connection string for table storage. dev-ops is looking to define a strategy for
                        // managing secrets by service, so not looking to add new secrets in the meantime
                        _configuration.GetConnectionString("IdentityTableStorage"),
                        sp.GetRequiredService<IOptionsMonitor<AzureStorageQueueOptions>>().CurrentValue),
                    async (@event, cancellationToken) =>
                    {
                        var mediator = sp.GetRequiredService<IMediator>();
                        await mediator.Command(new CreateOrUpdateFileBatchAddFileCommand
                        {
                            Uri = @event.Data.Url,
                            ETag = @event.Data.ETag,
                            ContentType = @event.Data.ContentType,
                            ContentLength = @event.Data.ContentLength,
                        }, cancellationToken);
                    },
                    sp.GetRequiredService<ILogger<AzureStorageQueueEventListener<FileUploadedToEscrowEvent>>>(),
                    messageDeserializer: DeserializeMessage));
        }

        private static AzureServiceBusTopicProvider GetTopicProvider(IConfiguration configuration)
        {
            return new AzureServiceBusTopicProvider(
                configuration.GetConnectionString("EventServiceBus"),
                configuration.GetSection("AzureServiceBus").Get<AzureServiceBusConfiguration>());
        }

        private static void AddSubscriptionListener<T>(EventListenerCollection eventListeners, IConfiguration configuration, Func<IServiceProvider, Func<T, CancellationToken, Task>> getEventHandler, string subscriptionSuffix = null, Expression<Func<T, bool>> filter = null)
        {
            eventListeners.Add(sp => new AzureServiceBusSubscriptionEventListener<T>(
                GetTopicProvider(configuration),
                "AdminPortal.Web" + (subscriptionSuffix != null ? "-" + subscriptionSuffix : ""),
                getEventHandler(sp),
                filter,
                sp.GetRequiredService<ILogger<AzureServiceBusSubscriptionEventListener<T>>>()));
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
            return _configuration.GetSection(LasoAuthenticationOptions.Section).Get<LasoAuthenticationOptions>().Enabled;
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            return
                string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) 
                || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal)
                || request.Path.StartsWithSegments("/api");
        }

        private class EventListenerCollection
        {
            private readonly ICollection<Func<IServiceProvider, IEventListener>> _eventListeners = new List<Func<IServiceProvider, IEventListener>>();

            public void Add(Func<IServiceProvider, IEventListener> eventListener)
            {
                _eventListeners.Add(eventListener);
            }

            public EventListenerHostedService GetHostedService(IServiceProvider serviceProvider)
            {
                return new EventListenerHostedService(_eventListeners.Select(x => x(serviceProvider)).ToList());
            }

            public class EventListenerHostedService : BackgroundService
            {
                private readonly ICollection<IEventListener> _eventListeners;

                public EventListenerHostedService(ICollection<IEventListener> eventListeners)
                {
                    _eventListeners = eventListeners;
                }

                protected override Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    return Task.WhenAll(_eventListeners.Select(x => x.Open(stoppingToken)));
                }
            }
        }
    }
}
