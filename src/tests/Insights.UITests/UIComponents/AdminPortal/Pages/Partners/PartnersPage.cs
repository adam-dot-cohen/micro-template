using System;
using Atata;
using System.Collections.Generic;
using System.Linq;
using Insights.UITests.TestData.Partners;
using OpenQA.Selenium;


namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersPage;

    [Url("partners")]
    [WaitForElement(WaitBy.XPath, "//*[text()='Partners']", Until.Visible, TriggerEvents.Init)]
    public class PartnersPage : Page<PartnersPage>
    {

        
        public ControlList<PartnerCard, _> PartnerItems { get; private set; }

        public Control<_> SnackBarPartnerSaved(string partner)
        {
            return Controls.Create<Control<_>>("snackbar partner page", new FindByXPathAttribute("//simple-snack-bar/span[contains(text(),'Saved partner: " + partner + "')]"));
        }

        public bool SnackBarPartnerProvisioned()
        {
          Control<_> we =
            Controls.Create<Control<_>>("snackbar partner provisioned", scopeLocator: new PlainScopeLocator(ByExtensions.Safely(By.XPath("//simple-snack-bar/span[contains(text(),'Partner provisioning complete!')]"))));
            if (we==null)
            {
                return false;
            }

            return true;
        }

        public T SelectPartnerCard<T>(Partner partner) where T : PageObject<T>
        {
            return
            FindPartnerCard(partner).
                PartnerName.ClickAndGo<T>(); 
            
        }

        public PartnerCard FindPartnerCard(Partner partner)
        {
            for (int i = 0; i < 3; i++)
            {
                int partnerItems = PartnerItems.Count;
                if (partnerItems > 0)
                {
                    break;
                }

                Wait(TimeSpan.FromSeconds(2));
            }
            
            return
            PartnerItems.Single(x => x.PartnerName.Attributes.TextContent.Value.Equals(partner.Name));
        }

        
        [ControlDefinition("div", ContainingClass = "mat-list-text", ComponentTypeName = "mat-list-text")]
        public class PartnerCard : Control<_>
        {

            public H4<_> PartnerName { get; private set; }

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

                IEnumerator<PartnerCard> e =
                    PartnerItems.GetEnumerator();

                while (e.MoveNext())
                {
                    if (e.Current != null)
                        partnersList.Add
                        (new Partner
                        {
                            ContactPhone = e.Current.ContactPhone.Attributes.TextContent.Value,
                            ContactEmail = e.Current.ContactEmail.Attributes.TextContent.Value,
                            Name = e.Current.PartnerName.Attributes.TextContent.Value,
                        });
                }

                return partnersList;
            }
        }

        public Partner FindPartner(Partner partner)
        {
            for (int i = 0; i < 3; i++)
            {
                int partnerItems = PartnersList.Count;
                if (partnerItems > 0)
                {
                    break;
                }

                Wait(TimeSpan.FromSeconds(2));
            }

            return
                PartnersList.Single(x => x.Name.Equals(partner.Name));
        }


    }
}