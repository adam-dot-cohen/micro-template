using System;
using System.Linq;
using Atata;
using Insights.UITests.TestData.Partners;


namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersDetailsPage;

    [Url("partners/detail")]
    [WaitForElement(WaitBy.XPath, "//div[@class='page-content']", Until.Visible, TriggerEvents.Init)]
    public class PartnersDetailsPage : Page<PartnersDetailsPage>
    {

        [FindByXPath("//*[contains(@class,'page-sub-header')]//mat-toolbar-row/span[1]")]
        public Control<_> MatCardTitle { get; private set; }

        public ControlList<PartnerCardDetails, _> PartnerDetails { get; private set; }


        public Partner PartnerOnDetailsPage
        {
            get {
                PartnerCardDetails partner = PartnerDetails.FirstOrDefault();
                if (partner == null)
                {
                    throw new Exception("there were no elements found to construct a partner on the details page");
                }

                return
                    new Partner
                    {
                        ContactName = partner.Name.Attributes.GetValue("ng-reflect-value"),
                        ContactEmail = partner.Email.Attributes.GetValue("ng-reflect-value"),
                        ContactPhone = partner.Phone.Attributes.GetValue("ng-reflect-value"),
                        Name = partner.PartnerTitle.Attributes.InnerHtml.Value
                    };
            }
        }

  
        [ControlDefinition("div[@class='page-content']//mat-card", ContainingClass = "mat-card",
            ComponentTypeName = "mat-card")]
        public class PartnerCardDetails : Control<_>
        {
            [FindByXPath("./../..//*[contains(@class,'page-sub-header')]//mat-toolbar-row/span[1]")]
            public  Control<_> PartnerTitle { get; private set; }

            [FindByXPath("input[@placeholder='Name']")]
            public Text<_> Name { get; private set; }

            [FindByXPath("input[@placeholder='Email']")]
            public Text<_> Email { get; private set; }

            [FindByXPath("input[@placeholder='Phone']")]
            public Text<_> Phone { get; private set; }

        }
    }



}