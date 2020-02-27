using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Core.Configuration
{
    public interface IEncryptionConfiguration
    {
        string QsPrivateCertificateName { get; }
        string QsPrivateCertificatePassPhrase { get; }
    }

    public class EncryptionConfiguration : IEncryptionConfiguration
    {
        private readonly IConfiguration _config;

        public EncryptionConfiguration(IConfiguration config)
        {
            _config = config;
        }

        public string QsPrivateCertificateName => _config["EncryptionConfiguration:QsPrivateCertificateName"];
        public string QsPrivateCertificatePassPhrase => _config["EncryptionConfiguration:QsPrivateCertificatePassPhrase"];
    }
}
