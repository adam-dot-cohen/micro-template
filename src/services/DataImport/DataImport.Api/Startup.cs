using System.Linq;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Laso.DataImport.Api.Mappers;
using Laso.DataImport.Api.Services;
using Laso.DataImport.Services.Encryption;
using Laso.DataImport.Services;
using Laso.DataImport.Services.Persistence;
using Laso.DataImport.Services.Persistence.Azure;
using Laso.DataImport.Services.Persistence.Azure.PropertyColumnMappers;
using Laso.DataImport.Services.Security;
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
            services.AddTransient<IConnectionStringConfiguration, ConnectionStringConfiguration>();
            services.AddTransient<IGrpcServiceEndpointConfiguration, GrpcServiceEndpointConfiguration>();
            services.AddTransient<IAzureKeyVaultConfiguration, AzureKeyVaultConfiguration>();
            services.AddTransient<IEncryptionConfiguration, EncryptionConfiguration>();
            services.AddTransient<IQuarterspotRepository, QuarterspotRepository>();
            services.AddTransient<IDataImporterFactory, DataImporterFactory>();
            services.AddTransient<IDataImporter, QsRepositoryDataImporter>();
            services.AddTransient<IDelimitedFileWriter, DelimitedFileWriter>();
            services.AddTransient<IPartnerService, DummyPartnerService>();
            services.AddTransient<ISecureStore, AzureKeyVaultSecureStore>();
            services.AddTransient<IImportSubscriptionsService, ImportSubscriptionsService>();
            services.AddTransient<IImportHistoryService, ImportHistoryService>();
            services.AddTransient<IBlobStorageService, AzureBlobStorageService>();


            services.AddTransient<ITableStorageContext>(x => new AzureTableStorageContext(
                Configuration["ConnectionStrings:ImportsTableStorageConnectionString"],
                "imports",
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
                }
            ));

            services.AddTransient<ITableStorageService, AzureTableStorageService>();

            services.AddTransient<IEntityMapperFactory, EntityMapperFactory>();
            AddAllImplementationsOf<IEntityMapper>(services, ServiceLifetime.Singleton);

            services.AddTransient<IEncryptionFactory, EncryptionFactory>();
            AddAllImplementationsOf<IEncryption>(services, ServiceLifetime.Transient);
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

                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client").ConfigureAwait(false); });
            });
        }

        private static void AddAllImplementationsOf<T>(IServiceCollection services, ServiceLifetime lifetime)
        {
            var implementations = typeof(T)
                .Assembly
                .DefinedTypes
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(T)));

            foreach (var impl in implementations)
                services.Add(new ServiceDescriptor(typeof(T), impl, lifetime));
        }
    }
}
