using System;
using System.Collections.Generic;
using Atata;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using Laso.Identity.Domain.Entities;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Insights.UITests.Tests.AdminPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("Partners")]
    public class PartnersTestsNUnit
    {
        [SetUp]
        public void SetUp()
        {
                AtataContext.Configure().
                    UseChrome().
                    UseBaseUrl("https://localhost:5001").
                    UseNUnitTestName().
                    AddNUnitTestContextLogging().
                    AddScreenshotFileSaving().
                    LogNUnitError().
                    TakeScreenshotOnNUnitError().
                    Build();
        }

        [TearDown]
        public void TearDown()
        {
            TestStatus testStatus =
            TestContext.CurrentContext.Result.Outcome.Status;
            Console.WriteLine( testStatus.ToString());
            Console.WriteLine(AtataContext.Current.Log);
            AtataContext.Current?.CleanUp();
           // AtataContext.Current.Driver.Close();//just to see access to driver.
        }


        [Test]
        public void CreatePartnerAllRequiredData()
        {
            UIComponent U =
                Go.To<PartnerPage>().PartnerName.Set("FI Partner Name")
                    .PrimaryContactName.Set("Ollie Parter")
                    .PrimaryContactEmail.Controls.Find(x=>x.Attributes.TextContent.Equals(""));
            //.PrimaryContactPhone.Set("512-255-3660")
            //.Save.Should.BeEnabled();
            //TODO: Validate partner is created either UI or API.
        }
     
        [Test]
        public void CreatePartnerAllRequiredDataTo()
        {
            new PartnerPage("fi partner", "ollie", "ollie@partner.com", "5125589999")
                .Create()

                .Save.Should.BeEnabled();

        }

     

        [Test]
        [TestCaseSource(nameof(TestCaseSourceData))]
        public void CreatePartnerRequiredFields(Partner partner)
        {
            Console.WriteLine(TestContext.CurrentContext.Test.Name);
            Go.To<PartnerPage>()
                .SetPartnerTO(partner)
                .CreateWithTO()
                .Wait(2)
                .Save.Should.BeEnabled(); //purposely failing to test individual screenshots.
           
        }

        public static IEnumerable<TestCaseData> TestCaseSourceData()
        {
            //4 iterations: FOR ERRORS ON SCREENSHOT TO BE CAPTURED INDIVIDUALLY NEED TO SET THE TEST CASE NAME TO SOMETHING MEANINGFUL AND DIFFERENT THAN THE DATA DRIVEN TEST CASE
            yield return new TestCaseData
                (new Partner
                    {Name = "", ContactName = "ollie", ContactEmail = "ollie@partner.com", ContactPhone = "5122533333"})
                .SetName("CreatePartnerRequiredFieldsNoName");
            yield return new TestCaseData(new Partner{Name ="fipartner", ContactName = "", ContactEmail = "ollie@partner.com", ContactPhone = "5122533333"})
                .SetName("CreatePartnerRequiredFieldsNoContactName")
                .SetProperty("ID","whatever");
                
            yield return new TestCaseData(new Partner { Name = "fipartner", ContactName = "contactname", ContactEmail = "", ContactPhone = "5122533333" })
                    .SetName("CreatePartnerRequiredFieldsNoContactEmail");
            yield return new TestCaseData(new Partner { Name = "fipartner", ContactName = "contactname", ContactEmail = "ollie@partner.com", ContactPhone = "" })
                .SetName("CreateParterRequiredFieldsNoContactPhone");
            

        }




    }
}
