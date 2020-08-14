using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Laso.Provisioning.Core.Messaging.SFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Provisioning.SFTP.Core.Account;
using Serilog;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IntegrationMessages.AzureServiceBus;
using Laso.IntegrationEvents;
using Laso.IntegrationEvents.AzureServiceBus;
using Laso.IntegrationMessages;
using Laso.IO.Serialization.Newtonsoft;
using Laso.Provisioning.Core;
using Laso.Provisioning.Infrastructure;
using Laso.Provisioning.SFTP.Core.Account;
using Mono.Unix;
using Mono.Unix.Native;

namespace Provisioning.SFTP.Worker
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configuration = GetConfiguration(args);

            LoggingConfig.Configure(configuration);

            try
            {
                Log.Information("Starting up");
                await ExecuteBootScript();
                //TODO: need to launch sshd if it isn't running
                //TODO: do we need to check if all partners are provisioned, etc?
                await CreateHostBuilder(configuration).Build().RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Start-up failed");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(IConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                        // Configure simple configuration for use during the host build process and
                        // in ConfigureAppConfiguration (or wherever the HostBuilderContext is
                        // supplied in the Host build process).
                        builder.AddConfiguration(configuration);
                })
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IEventPublisher>(sp =>
                    {
                        var topicProvider = new AzureServiceBusTopicProvider(
                            configuration.GetSection("Services:Provisioning:IntegrationEventHub").Get<AzureServiceBusConfiguration>(),
                            configuration["Services:Provisioning:IntegrationEventHub:ConnectionString"]);
                        return new AzureServiceBusEventPublisher(topicProvider, new DefaultMessageBuilder(new NewtonsoftSerializer()));
                    });

                    services.AddTransient<IApplicationSecrets>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var serviceUri = new Uri(configuration["Services:Provisioning:PartnerSecrets:ServiceUrl"]);
                        return new AzureKeyVaultApplicationSecrets(
                            new SecretClient(serviceUri, new DefaultAzureCredential()));
                    });

                    //Add Handlers and a Listener Service for each command
                    services.AddTransient<ICommandHandler<CreatePartnerAccountCommand>>(sp =>
                        new CreatePartnerAccountHandler(sp.GetRequiredService<IEventPublisher>(), hostContext.Configuration.GetValue<string>("StorageAccount")));
                    services.AddHostedService(sp => GetListenerService<CreatePartnerAccountCommand>(sp,configuration));

                    services.AddTransient<ICommandHandler<DeletePartnerAccountCommand>>(sp => new DeletePartnerAccountHandler(sp.GetRequiredService<IEventPublisher>()));
                    services.AddHostedService(sp => GetListenerService<DeletePartnerAccountCommand>(sp, configuration));

                    services.AddTransient<ICommandHandler<UpdatePartnerPasswordCommand>>(sp => new UpdatePartnerPasswordHandler(sp.GetRequiredService<IEventPublisher>()));
                    services.AddHostedService(sp => GetListenerService<UpdatePartnerPasswordCommand>(sp, configuration));

                    services.AddTransient<ICommandHandler<RotateAdminPasswordCommand>>(sp =>
                        new RotateAdminPasswordHandler(sp.GetRequiredService<IEventPublisher>(),
                            sp.GetRequiredService<IApplicationSecrets>(), Environment.MachineName));
                    services.AddHostedService(sp => GetListenerService<RotateAdminPasswordCommand>(sp, configuration));
                });

        private static IConfiguration GetConfiguration(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var configuration = builder.Build();

            return configuration;
        }

        private static AzureServiceBusQueueListener<T> GetListenerService<T>(IServiceProvider sp, IConfiguration configuration) where T : IIntegrationMessage
        {
            return new AzureServiceBusQueueListener<T>(
                sp.GetRequiredService<ILogger<AzureServiceBusQueueListener<T>>>(),
                sp.GetRequiredService<ICommandHandler<T>>(),
                new AzureServiceBusQueueProvider(configuration.GetSection("Services:Provisioning:IntegrationMessageHub").Get<AzureServiceBusMessageConfiguration>()), 
                new NewtonsoftSerializer());
        }

        private static async Task ExecuteBootScript()
        {
            var tmpDirectory = new UnixDirectoryInfo("/tmp/blobfuse");
            if(!tmpDirectory.Exists)
                tmpDirectory.Create(FilePermissions.ALLPERMS);

            var cmd = Cli.Wrap("/usr/local/bin/boot-blob-mount");
            var code = await cmd.ExecuteAsync(CancellationToken.None);
            Log.Information($"boot-blob-mount exited with code {code.ExitCode}");
        }
    }
}
