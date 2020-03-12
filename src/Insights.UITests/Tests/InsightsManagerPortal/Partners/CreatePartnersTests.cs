using System.Collections.Generic;
using Atata;
using Insights.UITests.TestData.Partners;
using Insights.UITests.Tests.AssertUtilities;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using NUnit.Framework;

namespace Insights.UITests.Tests.InsightsManagerPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("Partners")]
    public class CreatePartnersTests : TestFixtureBase
    {
        Partner expectedPartner = new Partner { ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com", Name = Randomizer.GetString("PartnerName{0}", 12) };
        private bool partnerCreated = false;
        
        [Test, Order(1)]
        public void CreatePartnerAllRequiredFields()
        {

            Partner actualPartner =
                Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<PartnersPage>()
                    .SnackBarPartnerSaved(expectedPartner.Name).Should.BeVisible()
                    .SnackBarPartnerSaved(expectedPartner.Name).Wait(Until.MissingOrHidden,options:new WaitOptions(8))
                    .FindPartner(expectedPartner);
 
            Assert.IsNotNull(actualPartner, "A partner should have been created with name" + expectedPartner.Name);

           new AssertObjectComparer<Partner>()
               .Compare(actualPartner, expectedPartner, new []{nameof(Partner.ContactName)});
           partnerCreated = true;

        }

        
        [Test]
        public void CannotCreatePartnerWithSameName()
        {
            if (!partnerCreated)
            {
                Assert.Inconclusive(AtataContext.Current.TestName + "is inconclusive as test case CreatePartnerAllRequiredFields did not succeed");
            }
            Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<CreatePartnerPage>()
                    .SnackBarPartnerAlreadyExists.Should.Exist();
        }


        [Test]
        public void PartnerDetailsValidation()
        {
            if (!partnerCreated)
            {
                Assert.Inconclusive(AtataContext.Current.TestName + "is inconclusive as test case CreatePartnerAllRequiredFields did not succeed");
            }

            Partner actualPartnerOnDetailsPage =
                Go.To<PartnersPage>()
                    .SelectPartnerCard<PartnersDetailsPage>(expectedPartner)
                    .PartnerOnDetailsPage;

            new AssertObjectComparer<Partner>()
                .Compare(actualPartnerOnDetailsPage, expectedPartner );

        }




        [TestCaseSource(nameof(PartnerTestData))]
        public void CreatePartnerMissingRequiredFields(Partner partner)
        {
            
            Go.To<CreatePartnerPage>()
                .Create(partner)
                .Wait(2)
                .SaveButton.Should.BeDisabled(); 
           
        }

        public static IEnumerable<TestCaseData> PartnerTestData()
        {

            yield return new TestCaseData(
                new Partner { ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com", Name = "" })
                .SetName("CreatePartnerRequiredFieldsNoPartnerName");
            yield return new TestCaseData(
                    new Partner { ContactName = "", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com", Name = "Partner Name" })
                .SetName("CreatePartnerRequiredFieldsNoContactName");
            yield return new TestCaseData(
                    new Partner { ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "", Name = "Partner Name" })
                .SetName("CreatePartnerRequiredFieldsNoContactEmail");
            yield return new TestCaseData(
                new Partner { ContactName = "Contact Name", ContactPhone = "", ContactEmail = "contact@partner.com", Name = "Partner Name" }).
                    SetName("CreatePartnerRequiredFieldsNoContactPhone");
                


        }

 



    }
}
