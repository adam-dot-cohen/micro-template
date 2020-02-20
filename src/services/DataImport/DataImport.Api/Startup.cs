using System.Linq;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Services;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Laso.DataImport.Api.Mappers;
using Laso.DataImport.Api.Services;
using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Api
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
