using Atata;
using System.Collections.Generic;
using Laso.AdminPortal.Web.Api.Partners;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersPage;

    [Url("partners")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='Partners']", Until.Visible, TriggerEvents.Init)]
    public class PartnersPage : Page<PartnersPage>
    {
        [ControlDefinition("table")] public Table<PartnersTableRow, _> PartnersTable { get; private set; }

        public class PartnersTableRow : TableRow<_>
        {
            public Text<_> PartnerName { get; private set; }

            public Text<_> ContactName { get; private set; }

            public Text<_> ContactEmail { get; private set; }

            public Text<_> ContactPhone { get; private set; }
        }

        public List<PartnerViewModel> PartnerList
        {
            
            get
            {
                var partnersList = new List<PartnerViewModel>();
                foreach (var row in PartnersTable.Rows)
                    partnersList.Add
                    (new PartnerViewModel
                    {
                        ContactName = row.ContactName.Attributes.TextContent,
                        ContactPhone = row.ContactPhone.Value,
                        ContactEmail = row.ContactEmail.Value,
                        Name = row.PartnerName.Value
                    });

                return partnersList;
            }
        }
    }
}