using System.Collections.Generic;
using Infrastructure.Mediation.Query;

namespace Laso.AdminPortal.Core.Partners.Queries
{
    public class GetPartnerProvisioningHistoryViewModelQuery : IQuery<PartnerProvisioningHistoryViewModel>
    {
        public string Id { get; set; }
    }

    public class PartnerProvisioningHistoryViewModel
    {
        public string Id { get; set; } 
        public string Name {get;set;}

        public IReadOnlyCollection<PartnerProvisioningEventViewModel> History {get;set;}
    }

    public class PartnerProvisioningEventViewModel
    {
        public string EventType { get; set; }
        public int Sequence { get; set; }
        public string Started { get; set; }
        public string Completed { get; set; }
        public bool Succeeded { get; set; }
        public string Errors { get; set; }
    }
}
