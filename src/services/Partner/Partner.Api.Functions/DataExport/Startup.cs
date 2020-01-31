using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Partner.Data.Quarterspot;
using Partner.Services.DataExport;

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
        }
    }
}
