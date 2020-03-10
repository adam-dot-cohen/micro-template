using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.KeyVault;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class PartnerConfigurationSetting
    {
        public string Category { get; set; }
        public bool IsSensitive { get; set; }
        public string Name { get; set; }
        public string KeyNameFormat { get; set; }
    }

    public class PartnerConfigurationSettings : List<PartnerConfigurationSetting>
    {
        public void Add(string category, bool isSensitive, string name, string keyNameFormat)
        {
            Add(new PartnerConfigurationSetting
            {
                Category = category,
                IsSensitive = isSensitive,
                Name = name,
                KeyNameFormat = keyNameFormat
            });
        }
    }

    public class GetPartnerConfigurationViewModelHandler : IQueryHandler<GetPartnerConfigurationViewModelQuery, PartnerConfigurationViewModel>
    {
        private readonly PartnerConfigurationSettings PartnerConfigurationSettings = new PartnerConfigurationSettings
        {
            { "FTP Configuration (Incoming/Outgoing)", false, "User Name", "{0}-partner-ftp-username" },
            { "FTP Configuration (Incoming/Outgoing)", true, "Password", "{0}-partner-ftp-password" },
            { "PGP Configuration (Incoming)", true, "Public Key", "{0}-laso-pgp-publickey" },
            { "PGP Configuration (Incoming)", true, "Private Key", "{0}-laso-pgp-privatekey" },
            { "PGP Configuration (Incoming)", true, "Passphrase", "{0}-laso-pgp-passphrase" },
            { "PGP Configuration (Outgoing)", true, "Public Key", "{0}-pgp-publickey" },
        };

        private readonly IApplicationSecrets _secrets;

        public GetPartnerConfigurationViewModelHandler(IApplicationSecrets secrets)
        {
            _secrets = secrets;
        }

        public async Task<QueryResponse<PartnerConfigurationViewModel>> Handle(GetPartnerConfigurationViewModelQuery query, CancellationToken cancellationToken)
        {
            var getSecretTasks = PartnerConfigurationSettings
                .Select(s => 
                    _secrets.GetSecret(string.Format(s.KeyNameFormat, query.PartnerId), cancellationToken))
                .ToList();

            var secrets = await Task.WhenAll(getSecretTasks);

            var model = new PartnerConfigurationViewModel
            {
                Id = query.PartnerId,
                Name = string.Empty,
                
                Settings = PartnerConfigurationSettings
                    .Select((s, i) => new PartnerConfigurationViewModel.ConfigurationSetting
                    {
                        Category = s.Category,
                        IsSensitive = s.IsSensitive,
                        Name = s.Name,
                        Value = secrets[i]?.Value
                    })
                    .ToList()
            };

            return QueryResponse.Succeeded(model);
        }
    }
}
