using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Data.Quarterspot;
using Laso.DataImport.Services;
using Laso.DataImport.Services.DTOs;
using Laso.DataImport.Services.Encryption;
using Laso.DataImport.Services.Imports;
using Laso.DataImport.Services.IO;
using Laso.DataImport.Services.IO.Storage.Blob.Azure;
using Laso.DataImport.Services.Security;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataImport.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options options)
        {
            var subscription = new ImportSubscription
            {
                EncryptionType = options.EncryptionType,
                Imports = options.Imports,
                Frequency = ImportFrequency.Weekly,
                OutputFileType = FileType.Csv,
                IncomingStorageLocation = options.OutputContainer,
                IncomingFilePath = options.OutputPath
            };

            var container = ConfigureServices();
            var importer = container.GetService<IDataImporter>();

            importer.ImportAsync(subscription).GetAwaiter().GetResult();
        }

        private static ServiceProvider ConfigureServices()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            // we need to get out base url before we can add the key vault provider... bit of a chick and the egg thing.
            var tmpConfig = configBuilder.Build();
            configBuilder.AddAzureKeyVault(tmpConfig["AzureKeyVault:VaultBaseUrl"], keyVaultClient, new DefaultKeyVaultSecretManager());
            
            var container = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IConnectionStringConfiguration, ConnectionStringConfiguration>()
                .AddSingleton<IAzureKeyVaultConfiguration, AzureKeyVaultConfiguration>()
                .AddSingleton<IEncryptionConfiguration, EncryptionConfiguration>()
                .AddSingleton<IQuarterspotRepository, QuarterspotRepository>()
                .AddSingleton<IDataImporter, QsRepositoryDataImporter>()
                .AddSingleton<IDelimitedFileWriter, DelimitedFileWriter>()
                .AddSingleton<ISecureStore, AzureKeyVaultSecureStore>()
                .AddSingleton<IBlobStorageService, AzureBlobStorageService>()
                .AddSingleton<IEncryptionFactory, EncryptionFactory>()
                .AddSingleton<IEncryption, PgpEncryption>()
                .AddSingleton<IEncryption, NoopEncryption>()
                .AddSingleton<IConfiguration>(configBuilder.Build())
                .BuildServiceProvider();

            return container;
        }
    }

    internal class Options
    {
        [Option('e', "encryption", Required = false, Default = EncryptionType.None, HelpText = "Ouput file encryption")]
        public EncryptionType EncryptionType { get; set; }

        [Option('o', "container", Required = false, Default = "insights", HelpText = "Output container name")]
        public string OutputContainer { get; set; }

        [Option('p', "path", Required = false, Default = "partner-Quarterspot/incoming/", HelpText = "Output file path")]
        public string OutputPath { get; set; }

        [Option('i', "imports", 
            Required = false, 
            Default = new[]
            {
                ImportType.Account, 
                ImportType.AccountTransaction, 
                ImportType.Demographic, 
                ImportType.Firmographic, 
                ImportType.LoanAccount, 
                ImportType.LoanApplication
            }, 
            HelpText = "Space separated list of import artifact types")]
        public IEnumerable<ImportType> Imports { get; set; }


    }
}
