using System.Collections.Generic;
using Infrastructure.Mediation.Query;

namespace Laso.AdminPortal.Core.Partners.Queries
{
    public class GetAllPartnerViewModelsQuery : IQuery<IReadOnlyCollection<PartnerViewModel>>
    {
    }

    public class PartnerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string PublicKey { get; set; }
    }
}