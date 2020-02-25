using Atata;
using Laso.Identity.Domain.Entities;

namespace Insights.UITests.Partners
{
    [Url("partners/create-partner")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='Create Partner']", Until.Visible, TriggerEvents.Init)]
    public class PartnerPage : Page<PartnerPage>
    {
        [FindById("name")]
        public TextInput<Partners.PartnerPage> PartnerName { get; private set; }

        [FindById("contactName")]
        public TextInput<Partners.PartnerPage> PrimaryContactName { get; private set; }

        [FindById("contactEmail")]
        public EmailInput<Partners.PartnerPage> PrimaryContactEmail { get; private set; }

        [FindById("contactPhone")]
        public TextInput<Partners.PartnerPage> PrimaryContactPhone { get; private set; }

        public Button<Partners.PartnerPage> Save { get; private set; }
        public Partner PartnerTO { get; private set; }

        public string Name { get; set; }
            public string ContactName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }

            public PartnerPage(Partner partnerTO)
            {
                PartnerTO = partnerTO;
            }

            //like this. helps for data comparison.
            public PartnerPage SetPartnerTO(Partner partner)
            {
                PartnerTO = partner;
             return this;

            }
            public PartnerPage(string name, string contactName, string email, string phone)
            {
                Name = name;
                ContactName = contactName;
                Email = email;
                Phone = phone;
            }

            public PartnerPage()
            {
            }

            public PartnerPage CreateWithTO()
            {
                return Go.To<PartnerPage>().PartnerName.Set(PartnerTO.Name)
                    .PrimaryContactName.Set(PartnerTO.ContactName)
                    .PrimaryContactEmail.Set(PartnerTO.ContactEmail)
                    .PrimaryContactPhone.Set(PartnerTO.ContactPhone)
                    .Save.Click();

            }

            public PartnerPage Create()
            {
                return Go.To<PartnerPage>().PartnerName.Set(Name)
                    .PrimaryContactName.Set(ContactName)
                    .PrimaryContactEmail.Set(Email)
                    .PrimaryContactPhone.Set(Phone)
                    .Save.Click();

            }

        public PartnerPage Create(Partner partner)
            {
                return Go.To<PartnerPage>().PartnerName.Set(partner.Name)
                    .PrimaryContactName.Set(partner.ContactName)
                    .PrimaryContactEmail.Set(partner.ContactEmail)
                    .PrimaryContactPhone.Set(partner.ContactPhone)
                    .Save.Click();

            }

 

    }

}