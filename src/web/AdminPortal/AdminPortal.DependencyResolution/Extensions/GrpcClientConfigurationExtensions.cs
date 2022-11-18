using System;
using Grpc.Net.Client.Web;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Infrastructure;
using Laso.Identity.Api.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Laso.AdminPortal.DependencyResolution.Extensions
{
    internal static class GrpcClientConfigurationExtensions
    {
        public static IServiceCollection AddIdentityServiceGrpcClient(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(IdentityServiceOptions.Section).Get<IdentityServiceOptions>();

            services.AddGrpcClient<Partners.PartnersClient>(opt => { opt.Address = new Uri(options.ServiceUrl); })
                // .ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()))

                // Force HTTP/1.1 since Azure App Service doesn't support 2.0 trailers
                .AddHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWebText))
                .AddHttpMessageHandler<BearerTokenHandler>();

            return services;
        }

        public static IServiceCollection AddProvisioningServiceGrpcClient(this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration.GetSection(ProvisioningServiceOptions.Section)
                .Get<ProvisioningServiceOptions>();

            services.AddGrpcClient<Laso.Provisioning.Api.V1.Partners.PartnersClient>("ProvisioningClient",opt =>
                    {
                        opt.Address = new Uri(options.ServiceUrl);
                    })
                .AddHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWebText))
                .AddHttpMessageHandler<BearerTokenHandler>();

            return services;
        }
    }
}