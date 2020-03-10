using System;
using Atata;
using Insights.UITests.TestData.Partners;
using Insights.UITests.UIComponents.AdminPortal.Pages.Login;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = CreatePartnerPage;
    [Url("partners/create")]
    [WaitForElement(WaitBy.XPath, "//mat-card-title[text()='New Partner']", Until.Visible, TriggerEvents.Init)]
    public class CreatePartnerPage : Page<CreatePartnerPage>
    {
        [FindByAttribute("test-id","name"), FindById("name")]
        public TextInput<_> PartnerName { get; private set; }

        [FindByAttribute("test-id","contactName")]
        public TextInput<_> PrimaryContactName { get; private set; }

        [FindByAttribute("test-id","contactEmail")]
        public EmailInput<_> PrimaryContactEmail { get; private set; }

        [FindByAttribute("test-id","contactPhone")]
        public TextInput<_> PrimaryContactPhone { get; private set; }

        public Button<_> SaveButton { get; private set; }

        [FindByXPath("simple-snack-bar/span[contains(text(),'Partner already exists')]")]
        public Control<_> SnackBarPartnerAlreadyExists { get; private set; }
       

        public T Save<T>() where T : PageObject<T>
        { 
            //SaveButton.ClickAndGo<T>();
                SaveButton.Click();
                return Go.To<T>(navigate:false );
        }


        public _ Create(Partner partner)
        {
            return Go.To<_>().PartnerName.Set(partner.Name)
                .PrimaryContactName.Set(partner.ContactName)
                .PrimaryContactEmail.Set(partner.ContactEmail)
                .PrimaryContactPhone.Set(partner.ContactPhone);


        }


 
    }

}