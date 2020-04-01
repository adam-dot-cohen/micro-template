using System;
using System.Collections.Generic;
using System.Linq;
using Atata;
using Insights.UITests.TestData.Partners;
using Insights.UITests.Tests.AssertUtilities;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Insights.UITests.Tests.InsightsManagerPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("Partners")]
    public class CreatePartnersTests : TestFixtureBase
    {
        private Partner expectedPartner = new Partner
        {
            ContactName = "Partner Conact Name", 
            ContactPhone = "512-2553633",
            ContactEmail = "contact@partner.com", 
            Name = Randomizer.GetString("PartnerName{0}", 12) };
        private bool _partnerCreated;
        private bool _partnerProvisioned;
        
        [Test, Order(1)]
        public void CreateAndProvisionPartnerAllRequiredFields()
        {

            PartnersPage partnersPage=
                Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<PartnersPage>()
                    .SnackBarPartnerSaved(expectedPartner.Name).Should.BeVisible()
                    .SnackBarPartnerSaved(expectedPartner.Name).Wait(Until.MissingOrHidden,options:new WaitOptions(12));
         
         _partnerProvisioned =
         partnersPage.SnackBarPartnerProvisioned();

         Partner  actualPartner =
         partnersPage.FindPartner(expectedPartner);

         Assert.IsNotNull(actualPartner, "A partner should have been created with name" + expectedPartner.Name);

         new AssertObjectComparer<Partner>()
               .Compare(actualPartner, expectedPartner, new []{nameof(Partner.ContactName)});
           _partnerCreated = true;

        }

        
        [Test]
        public void CannotCreatePartnerWithSameName()
        {
            if (!_partnerCreated)
            {
                Assert.Inconclusive(AtataContext.Current.TestName + "is inconclusive as test case CreateAndProvisionPartnerAllRequiredFields did not succeed");
            }
            Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<CreatePartnerPage>()
                    .SnackBarPartnerAlreadyExists.Should.Exist();
        }


        [Test]
        public void PartnerDetailsValidation()
        {
            if (!_partnerCreated)
            {
                Assert.Inconclusive(AtataContext.Current.TestName + "is inconclusive as test case CreateAndProvisionPartnerAllRequiredFields did not succeed");
            }

            Partner actualPartnerOnDetailsPage =
                Go.To<PartnersPage>()
                    .SelectPartnerCard<PartnersDetailsPage>(expectedPartner)
                    .PartnerOnDetailsPage;
            new AssertObjectComparer<Partner>()
                .Compare(actualPartnerOnDetailsPage, expectedPartner );
        }


        [Test]
        public void PartnerConfigurationValidation()
        {
            if (!_partnerProvisioned)
            {
                Assert.Inconclusive(AtataContext.Current.TestName + "is inconclusive as test case CreateAndProvisionPartnerAllRequiredFields did not succeed. Partner was not provisioned configuration cannot be validated");
            }

            List<TestData.Partners.Configuration> partnerConfiguration =

                Go.To<PartnersPage>()
                    .SelectPartnerCard<PartnersDetailsPage>(expectedPartner)
                    .SelectViewPartnerConfiguration()
                    .ConfigurationOnPartnerConfigurationPage();
            
            
            Configuration configItem =
            partnerConfiguration.Single(x => x.Name.Equals("User Name")&& x.Category.Equals("FTP Configuration (Incoming/Outgoing)"));
            Assert.True(configItem.Value.Contains(expectedPartner.Name.Replace(" ","").ToLowerInvariant()),"Validating the Vale for User Name configuration Name includes the partner name");
            configItem =
                partnerConfiguration.Single(x => x.Name.Equals("Password") && x.Category.Equals("FTP Configuration (Incoming/Outgoing)"));
            Assert.False(String.IsNullOrEmpty(configItem.Value), "The password configuration item should have a value ");
            configItem =
                partnerConfiguration.Single(x => x.Name.Equals("Public Key") && x.Category.Equals("PGP Configuration (Incoming)"));
            Assert.True(configItem.Value.Contains("BEGIN PGP PUBLIC KEY BLOCK"), "Validating the Public Key PGP Configuration (Incoming) was set to a value starting with -BEGIN PGP PUBLIC KEY BLOCK ");

            configItem =
                partnerConfiguration.Single(x => x.Name.Equals("Private Key") && x.Category.Equals("PGP Configuration (Incoming)"));
            Assert.True(configItem.Value.Contains("BEGIN PGP PRIVATE KEY BLOCK"), "Validating the Public Key PGP Configuration (Incoming) was set to a value starting with -BEGIN PGP PRIVATE KEY BLOCK ");

            configItem =
                partnerConfiguration.Single(x => x.Name.Equals("Passphrase") && x.Category.Equals("PGP Configuration (Incoming)"));
            Assert.False(String.IsNullOrEmpty(configItem.Value), "The passphrase configuration item should have a value ");

            configItem =
                partnerConfiguration.Single(x => x.Name.Equals("Public Key") && x.Category.Equals("PGP Configuration (Outgoing)"));
            Assert.False(String.IsNullOrEmpty(configItem.Value), "The PGP Configuration (Outgoing) public key configuration item should have a value ");
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
