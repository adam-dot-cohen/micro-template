using DataImport.Api.Functions.Configuration;
using DataImport.Core.Configuration;
using DataImport.Data.Quarterspot;
using DataImport.Services.DataImport;
using DataImport.Services.IO;
using DataImport.Services.IO.Storage.Blob.Azure;
using DataImport.Services.Partners;
using DataImport.Services.Subscriptions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DataImport.Api.Functions.Import.Startup))]

namespace DataImport.Api.Functions.Import
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
            builder.Services.AddTransient<IBlobStorageConfiguration, BlobStorageConfiguration>();
            builder.Services.AddTransient<IPartnerService, PartnerService>();
            builder.Services.AddTransient<IImportSubscriptionsService, ImportSubscriptionsService>();
            builder.Services.AddTransient<IBlobStorageService>(x =>
            {
                var config = x.GetRequiredService<IBlobStorageConfiguration>();
                return new AzureBlobStorageService(config, config.LasoBlobStorageConnectionString);
            });
        }
    }
}
