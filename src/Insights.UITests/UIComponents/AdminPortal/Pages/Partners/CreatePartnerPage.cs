using Atata;
using Laso.Identity.Domain.Entities;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = CreatePartnerPage;
    [Url("partners/create-partner")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='Create Partner']", Until.Visible, TriggerEvents.Init)]
    public class CreatePartnerPage : Page<CreatePartnerPage>
    {
        [FindById("name")]
        public TextInput<_> PartnerName { get; private set; }

        [FindById("contactName")]
        public TextInput<_> PrimaryContactName { get; private set; }

        [FindById("contactEmail")]
        public EmailInput<_> PrimaryContactEmail { get; private set; }

        [FindById("contactPhone")]
        public TextInput<_> PrimaryContactPhone { get; private set; }

        public Button<_> Save { get; private set; }
        public Partner PartnerEntityTestObject { get; private set; }

        public string Name { get; set; }
            public string ContactName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }

  
            //like this. helps for data comparison.
            public CreatePartnerPage SetPartnerEntityTestObject(Partner partner)
            {
                PartnerEntityTestObject = partner;
             return this;

            }
 

            public CreatePartnerPage CreateWithTestEntityTestObject()
            {
                return Go.To<CreatePartnerPage>().PartnerName.Set(PartnerEntityTestObject.Name)
                    .PrimaryContactName.Set(PartnerEntityTestObject.ContactName)
                    .PrimaryContactEmail.Set(PartnerEntityTestObject.ContactEmail)
                    .PrimaryContactPhone.Set(PartnerEntityTestObject.ContactPhone)
                    .Save.Click();

            }


        public T Create<T>() where T : PageObject<T>
        {
            return Go.To<_>().PartnerName.Set(PartnerEntityTestObject.Name)
                .PrimaryContactName.Set(PartnerEntityTestObject.ContactName)
                .PrimaryContactEmail.Set(PartnerEntityTestObject.ContactEmail)
                .PrimaryContactPhone.Set(PartnerEntityTestObject.ContactPhone)
                .Save.ClickAndGo<T>();

        }


        public CreatePartnerPage Create()
            {
                return Go.To<CreatePartnerPage>().PartnerName.Set(Name)
                    .PrimaryContactName.Set(ContactName)
                    .PrimaryContactEmail.Set(Email)
                    .PrimaryContactPhone.Set(Phone)
                    .Save.Click();

            }

        public CreatePartnerPage Create(Partner partner)
            {
                return Go.To<CreatePartnerPage>().PartnerName.Set(partner.Name)
                    .PrimaryContactName.Set(partner.ContactName)
                    .PrimaryContactEmail.Set(partner.ContactEmail)
                    .PrimaryContactPhone.Set(partner.ContactPhone)
                    .Save.Click();

            }

 
    }

}