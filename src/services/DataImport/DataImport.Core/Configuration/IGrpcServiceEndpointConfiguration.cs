using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Core.Configuration
{
    public interface IGrpcServiceEndpointConfiguration
    {
        string PartnersEndpoint { get; }
    }

    public class GrpcServiceEndpointConfiguration : IGrpcServiceEndpointConfiguration
    {
        private readonly IConfiguration _appConfig;

        public GrpcServiceEndpointConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        public string PartnersEndpoint => _appConfig["ServiceEndpoints:PartnersService"];
    }
}
