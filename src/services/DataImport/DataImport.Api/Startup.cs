using System.Linq;
using DataImport.Core.Configuration;
using DataImport.Data.Quarterspot;
using DataImport.Services.Imports;
using DataImport.Services.IO;
using DataImport.Services.IO.Storage.Blob.Azure;
using DataImport.Services.Partners;
using DataImport.Services.SubscriptionHistory;
using DataImport.Services.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataImport.Api.Configuration;
using DataImport.Api.Mappers;
using DataImport.Api.Services;
using Microsoft.Extensions.Configuration;

namespace DataImport.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddOptions();

            services.AddSingleton(Configuration);
            services.Configure<ConnectionStringConfiguration>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<GrpcServiceEndpointConfiguration>(Configuration.GetSection("ServiceEndpoints"));

            services.AddTransient<IQuarterspotRepository, QuarterspotRepository>();
            services.AddTransient<IDataImporterFactory, DataImporterFactory>();
            services.AddTransient<IDataImporter, QsRepositoryDataImporter>();
            services.AddTransient<IDelimitedFileWriter, DelimitedFileWriter>();
            //services.AddTransient<IPartnerService, PartnerService>();
            services.AddTransient<IPartnerService, DummyPartnerService>();
            services.AddTransient<IImportSubscriptionsService, ImportSubscriptionsService>();
            services.AddTransient<IImportHistoryService, ImportHistoryService>();
            services.AddTransient<IBlobStorageService>(x =>
            {
                var config = Configuration.GetSection("ConnectionStrings").Get<ConnectionStringConfiguration>();
                return new AzureBlobStorageService(config.LasoBlobStorageConnectionString);
            });

            services.AddTransient<IDtoMapperFactory, DtoMapperFactory>();

            var mappers = typeof(Startup).Assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(IDtoMapper)));
            foreach (var mapper in mappers)
                services.Add(new ServiceDescriptor(typeof(IDtoMapper), mapper, ServiceLifetime.Singleton));
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ImportService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client").ConfigureAwait(false);
                });
            });
        }
    }
}
