using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Partner.Api.Functions.Configuration;
using Partner.Core.Configuration;
using Partner.Data.Quarterspot;
using Partner.Services.DataExport;
using Partner.Services.IO;
using Partner.Services.IO.Storage;

[assembly: FunctionsStartup(typeof(Partner.Api.Functions.DataExport.Startup))]

namespace Partner.Api.Functions.DataExport
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();

            builder.Services.AddTransient<IQuarterspotRepository, QuarterspotRepository>();
            builder.Services.AddTransient<IDataExporterFactory, DataExporterFactory>();
            builder.Services.AddTransient<IDataExporter, QsRepositoryDataExporter>();
            builder.Services.AddTransient<IApplicationConfiguration, AzureFunctionsConfiguration>();
            builder.Services.AddTransient<IDelimitedFileWriter, DelimitedFileWriter>();
            builder.Services.AddTransient<IStorageMonikerFactory, StorageMonikerFactory>();
            builder.Services.AddTransient<IFileStorageService, LocalFileSystemStorageService>();
        }
    }
}
