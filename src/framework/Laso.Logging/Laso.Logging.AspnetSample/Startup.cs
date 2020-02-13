using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laso.Logging.Configuration;
using Laso.Logging.Configuration.Enrichers;
using Laso.Logging.Extensions;
using Laso.Logging.Seq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;

namespace Laso.Logging.AspnetSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //AddLogging is an extension method that pipes into the ASP.NET Core service provider.  
            // YOu can peek it and implement accordingly if your use case is different, but this makes it easy for the common use cases. 
            services.AddLogging(BuildLoggingConfiguration());

        }

        private static LoggingConfiguration BuildLoggingConfiguration()
        {
            //Build the settings from config ( not required, but easier - this is just a sample)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            var loggingSettings = new LoggingSettings();
            configuration.GetSection("Laso:Logging:Common").Bind(loggingSettings);

            var seqSettings = new SeqSettings();
            configuration.GetSection("Laso:Logging:Seq").Bind(seqSettings);



            return  new LoggingConfigurationBuilder()
                .BindTo(new SeqSinkBinder(seqSettings))
                .Build(x => x.Enrich.ForLaso(loggingSettings));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //set up Logging of the HTTP Request and add said values to any down stream value from there. 
            app.ConfigureRequestLoggingOptions();
        

        }

       
    }
}
