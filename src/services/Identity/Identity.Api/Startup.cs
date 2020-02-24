using System.IO;
using Laso.Identity.Api.Configuration;
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
            builder.AddInMemoryClients(Config.GetClients());

            // or in-memory, json config
            //builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            //builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            //builder.AddInMemoryClients(Configuration.GetSection("clients"));

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddSigningCredential(Certificate.Get());

            // services.AddAuthentication();
            // services.AddAuthorization();
            services.AddMvc();
            services.AddSingleton<CustomSetting>(x =>
            {

                //Build the settings from config ( not required, but easier - this is just a sample)
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var configurationWithOverride = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                return new CustomSetting
                {
                    DeveloperValue = configuration.GetValue<string>("Laso:CustomValue"),
                    DeveloperValueOverridden = configurationWithOverride.GetValue<string>("Laso:CustomValue"),
                };
            });
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
                endpoints.MapGrpcService<GreeterService>();
                
                // endpoints.MapGet("/", async context =>
                // {
                    // await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                // });
            });
        }
    }

    public class CustomSetting  
    {
        public string DeveloperValue { get; set; }
        public string DeveloperValueOverridden { get; set; }
    }
}
