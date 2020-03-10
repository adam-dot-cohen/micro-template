using Atata;


namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersDetailsPage;

    [Url("partners/detail")]
    [WaitForElement(WaitBy.XPath, "//div[@class='page-content']", Until.Visible, TriggerEvents.Init)]
    public class PartnersDetailsPage : Page<PartnersDetailsPage>
    {

        [FindByXPath("//mat-card[contains(@class,'page-sub-header')]//mat-card-header//mat-card-title")]
        public Control<_> MatCardTitle { get; private set; }

        public ControlList<MatCard, _> PartnerDetails { get; private set; }



        [ControlDefinition("div[@class='page-content']//mat-card", ContainingClass = "mat-card",
            ComponentTypeName = "mat-card")]
        public class MatCard : Control<_>
        {
            [FindByXPath("input[@placeholder='Name']")]
            public Text<_> Name { get; private set; }

            [FindByXPath("input[@placeholder='Email']")]
            public Text<_> Email { get; private set; }

            [FindByXPath("input[@placeholder='Phone']")]
            public Text<_> Phone { get; private set; }

        }
    }



}