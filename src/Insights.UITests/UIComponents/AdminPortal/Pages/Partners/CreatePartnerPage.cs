using Atata;
using Laso.AdminPortal.Web.Api.Partners;
using Laso.Identity.Domain.Entities;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = CreatePartnerPage;
    [Url("partners/create-partner")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='Create Partner']", Until.Visible, TriggerEvents.Init)]
    public class CreatePartnerPage : Page<CreatePartnerPage>
    {
        [FindByClass,FindById("name"),]
        public TextInput<_> PartnerName { get; private set; }

        [FindById("contactName")]
        public TextInput<_> PrimaryContactName { get; private set; }

        [FindById("contactEmail")]
        public EmailInput<_> PrimaryContactEmail { get; private set; }

        [FindById("contactPhone")]
        public TextInput<_> PrimaryContactPhone { get; private set; }

        public Button<_> SaveButton { get; private set; }
        public PartnerViewModel PartnerTestObject { get; private set; }

        public string Name { get; set; }
            public string ContactName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }





        public T Save<T>() where T : PageObject<T>
        {
            return
            SaveButton.ClickAndGo<T>();

        }


   

        public _ Create(PartnerViewModel partner)
            {
                return Go.To<_>().PartnerName.Set(partner.Name)
                    .PrimaryContactName.Set(partner.ContactName)
                    .PrimaryContactEmail.Set(partner.ContactEmail)
                    .PrimaryContactPhone.Set(partner.ContactPhone)
                

            }


 
    }

}