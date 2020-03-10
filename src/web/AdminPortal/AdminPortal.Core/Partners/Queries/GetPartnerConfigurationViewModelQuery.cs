﻿using System.Collections.Generic;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Partners.Queries
{
    public class GetPartnerConfigurationViewModelQuery : IQuery<PartnerConfigurationViewModel>
    {
        public string PartnerId { get; set; }
    }

    public class PartnerConfigurationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public IReadOnlyCollection<ConfigurationSetting> Settings { get; set; }
        
        public class ConfigurationSetting
        {
            public string Category { get; set; }
            public bool IsSensitive { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
