using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Core.Partners.Queries
{
    public class GetPartnerViewModelQuery : IQuery<PartnerViewModel>
    {
        public string PartnerId { get; set; }
    }
}