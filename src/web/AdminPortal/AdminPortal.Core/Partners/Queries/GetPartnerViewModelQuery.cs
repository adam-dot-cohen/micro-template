using Laso.Mediation;

namespace Laso.AdminPortal.Core.Partners.Queries
{
    public class GetPartnerViewModelQuery : IQuery<PartnerViewModel>
    {
        public string PartnerId { get; set; }
    }
}