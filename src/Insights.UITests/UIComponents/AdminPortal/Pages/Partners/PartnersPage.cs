using System;
using Atata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Insights.UITests.TestData.Partners;
using Microsoft.Azure.Amqp.Framing;


namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersPage;

    [Url("partners")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='Partners']", Until.Visible, TriggerEvents.Init)]
    public class PartnersPage : Page<PartnersPage>
    {

        
        public ControlList<MatListText, _> PartnerItems { get; private set; }

        public Control<_> SnackBarPartnerSaved(string partner)
        {
            return Controls.Create<Control<_>>("", new FindByXPathAttribute("//simple-snack-bar/span[contains(text(),'Saved partner: " + partner + "')]"));
        }

        public MatListText FindPartner(Partner partner)
        {
            return
            PartnerItems.Single(x => x.PartnerName.Attributes.TextContent.Value.Equals(partner.Name));
        }

        
        [ControlDefinition("div", ContainingClass = "mat-list-text", ComponentTypeName = "mat-list-text")]
        public class MatListText : Control<_>
        {

            public H4<_> PartnerName { get; private set; }

            [FindByXPath("p[1]")]
            public Text<_> ContactName { get; private set; }

            [FindByXPath("p[1]")]
            public Text<_> ContactEmail { get; private set; }

            [FindByXPath("p[2]")]
            public Text<_> ContactPhone { get; private set; }
        }


        
        public List<Partner> PartnersList
        {
            get
            {
                var partnersList = new List<Partner>();

                IEnumerator<MatListText> e =
                    PartnerItems.GetEnumerator();

                while (e.MoveNext())
                {
                    if (e.Current != null)
                        partnersList.Add
                        (new Partner
                        {
                            ContactName = e.Current.ContactName.Attributes.TextContent.Value,
                            ContactPhone = e.Current.ContactPhone.Attributes.TextContent.Value,
                            ContactEmail = e.Current.ContactEmail.Attributes.TextContent.Value,
                            Name = e.Current.PartnerName.Attributes.TextContent.Value,
                        });
                }

                return partnersList;
            }
        }
 
    }
}