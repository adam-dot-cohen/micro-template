using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Partner.Api.Functions.Configuration;
using Partner.Core.Configuration;
using Partner.Data.Quarterspot;
using Partner.Services.DataImport;
using Partner.Services.IO;
using Partner.Services.IO.Storage.Blob.Azure;

[assembly: FunctionsStartup(typeof(Partner.Api.Functions.DataImport.Startup))]

namespace Partner.Api.Functions.DataImport
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();

            builder.Services.AddTransient<IQuarterspotRepository, QuarterspotRepository>();
            builder.Services.AddTransient<IDataImporterFactory, DataImporterFactory>();
            builder.Services.AddTransient<IDataImporter, QsRepositoryDataImporter>();
            builder.Services.AddTransient<IDelimitedFileWriter, DelimitedFileWriter>();
            builder.Services.AddTransient<IStorageMonikerFactory, StorageMonikerFactory>();
            builder.Services.AddTransient<IConnectionStringsConfiguration, ConnectionStringsConfiguration>();
            builder.Services.AddTransient<IImportPathResolver, LasoImportPathResolver>();
            builder.Services.AddTransient<IBlobStorageConfiguration, BlobStorageConfiguration>();
            builder.Services.AddTransient<IBlobStorageService>(x =>
            {
                var config = x.GetRequiredService<IBlobStorageConfiguration>();
                return new AzureBlobStorageService(config, config.LasoBlobStorageConnectionString);
            });
        }
    }
}
