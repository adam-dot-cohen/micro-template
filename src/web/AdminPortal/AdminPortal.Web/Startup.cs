using System;
using System.IdentityModel.Tokens.Jwt;
using Laso.AdminPortal.Web.Configuration;
using Laso.AdminPortal.Web.Events;
using Laso.AdminPortal.Web.Hubs;
using Laso.Logging.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ServicesOptions>(Configuration.GetSection(ServicesOptions.Section));
            services.Configure<IdentityServiceOptions>(Configuration.GetSection(IdentityServiceOptions.Section));
            services.Configure<AuthenticationOptions>(Configuration.GetSection(AuthenticationOptions.Section));

            // Enable Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            services.AddSignalR();
            services.AddControllers();

            const string SignInScheme = "Cookies";
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = SignInScheme;
                    options.DefaultChallengeScheme = "oidc";
                }).AddCookie(SignInScheme)
                // .AddCookie("Cookies", options =>
                // {
                    // options.AccessDeniedPath = "/Authorization/AccessDenied";
                // })
                .AddOpenIdConnect("oidc", options =>
                {
                    var authOptions = Configuration.GetSection(AuthenticationOptions.Section).Get<AuthenticationOptions>();
                    options.SignInScheme = SignInScheme;
                    options.Authority = authOptions.AuthorityUrl;
                    // RequireHttpsMetadata = false;
                    options.ClientId = authOptions.ClientId;
                    options.ClientSecret = authOptions.ClientSecret;
                    options.ResponseType = "code id_token"; // Hybrid flow
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("email");
                    options.Scope.Add("identity");
                    options.SaveTokens = true;
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // AddLogging is an extension method that pipes into the ASP.NET Core service provider.  
            // You can peek it and implement accordingly if your use case is different, but this makes it easy for the common use cases. 
            // services.AddLogging(BuildLoggingConfiguration());

            services.AddHostedService(x => new AzureServiceBusEventSubscriptionListener<ProvisioningCompletedEvent>(Configuration.GetConnectionString("EventServiceBus"), "adminWebPortal", y => { }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            // app.UseSerilogRequestLogging();
            app.ConfigureRequestLoggingOptions();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            // Use claim types as we define them rather than mapping them to url namespaces
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseEndpoints(endpoints =>
            {
                // Don't define routes, will use attribute routing
                endpoints.MapControllers();
                endpoints.MapHub<NotificationsHub>("/hub/notifications");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
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
        //     Configuration.GetSection("Laso:Logging:Common").Bind(loggingSettings);
        //
        //     var seqSettings = new SeqSettings();
        //     Configuration.GetSection("Laso:Logging:Seq").Bind(seqSettings);
        //
        //     var logglySettings = new LogglySettings();
        //     Configuration.GetSection("Laso:Logging:Loggly").Bind(logglySettings);
        //
        //     return  new LoggingConfigurationBuilder()
        //         .BindTo(new SeqSinkBinder(seqSettings))
        //         .BindTo(new LogglySinkBinder(loggingSettings, logglySettings))
        //         .Build(x => x.Enrich.ForLaso(loggingSettings));
        // }
    }
}
