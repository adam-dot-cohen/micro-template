using System;
using Atata;
using NUnit.Framework;
using NUnit.Framework.Interfaces;



namespace Insights.UITests.Partners
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"),Category("Partners")]
    public class PartnersTestsNUnitPar
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
            
            
        }


        [Test]
        public void CreatePartnerAllRequiredData()
        {
            Go.To<PartnerPage>().PartnerName.Set("FI Partner Name")
                .PrimaryContactName.Set("Ollie Parter")
                .PrimaryContactEmail.Set("ollie@partner.com")
                .PrimaryContactPhone.Set("512-255-3660")
                .Save.Should.BeEnabled();
            //TODO: Validate partner is created either UI or API.
        }


    
    


    }
}
