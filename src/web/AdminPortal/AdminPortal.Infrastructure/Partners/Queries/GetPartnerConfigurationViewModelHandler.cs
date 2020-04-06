using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.Identity.Api.V1;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetPartnerConfigurationViewModelHandler : IQueryHandler<GetPartnerConfigurationViewModelQuery, PartnerConfigurationViewModel>
    {
        public static readonly PartnerConfigurationSettings PartnerConfigurationSettings = new PartnerConfigurationSettings
        {
            { "FTP Configuration (Incoming/Outgoing)", false, "User Name", "{0}-partner-ftp-username" },
            { "FTP Configuration (Incoming/Outgoing)", true, "Password", "{0}-partner-ftp-password" },
            { "PGP Configuration (Incoming)", true, "Public Key", "{0}-laso-pgp-publickey" },
            { "PGP Configuration (Incoming)", true, "Private Key", "{0}-laso-pgp-privatekey" },
            { "PGP Configuration (Incoming)", true, "Passphrase", "{0}-laso-pgp-passphrase" },
            { "PGP Configuration (Outgoing)", true, "Public Key", "{0}-pgp-publickey" }
        };

        private readonly IApplicationSecrets _applicationSecrets;
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public GetPartnerConfigurationViewModelHandler(
            IApplicationSecrets applicationSecrets,
            Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _applicationSecrets = applicationSecrets;
            _partnersClient = partnersClient;
        }

        public async Task<QueryResponse<PartnerConfigurationViewModel>> Handle(GetPartnerConfigurationViewModelQuery query, CancellationToken cancellationToken)
        {
            var getSecretTasks = PartnerConfigurationSettings
                .Select(s => 
                    _applicationSecrets.GetSecret(string.Format(s.KeyNameFormat, query.Id), cancellationToken))
                .ToList();
            var secrets = await Task.WhenAll(getSecretTasks);

            var partnerReply = await _partnersClient.GetPartnerAsync(new GetPartnerRequest { Id = query.Id });

            var model = new PartnerConfigurationViewModel
            {
                Id = query.Id,
                Name = partnerReply.Partner.Name,
                
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
}
