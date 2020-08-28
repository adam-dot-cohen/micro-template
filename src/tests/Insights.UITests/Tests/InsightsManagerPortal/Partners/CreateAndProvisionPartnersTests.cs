using System.Collections.Generic;
using System.Linq;
using Atata;
using Insights.UITests.TestData.Partners;
using Insights.UITests.Tests.AssertUtilities;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using NUnit.Framework;

namespace Insights.UITests.Tests.InsightsManagerPortal.Partners
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke")]
    [Category("Partners")]
    public class CreateAndProvisionPartnersTests : TestFixtureBase
    {
        private readonly Partner expectedPartner = new Partner
        {
            ContactName = "Partner Conact Name",
            ContactPhone = "512-2553633",
            ContactEmail = "contact@partner.com",
            Name = Randomizer.GetString("PartnerName{0}", 12)
        };

        private bool _partnerProvisioned;
        private bool _partnerSaved;
        private bool _partnerListed;
        private bool _doneSetup;

        [SetUp]
        public void CreateNewPartnerAllRequiredFields()
        {
            if (!_doneSetup)
            {
                var partnersPage =
                    Go.To<CreatePartnerPage>()
                        .Create(expectedPartner)
                        .Save<PartnersPage>()
                        .SnackBarPartnerSaved(expectedPartner.Name).Should.BeVisible()
                        .SnackBarPartnerSaved(expectedPartner.Name)
                        .Wait(Until.MissingOrHidden, new WaitOptions(12));
                _partnerSaved = true;
                _partnerProvisioned =
                    partnersPage.SnackBarPartnerProvisioned();
            }

            _doneSetup = true;
        }

        [Test]
        public void ValidateSnackbarPartnerSaved()
        {
            Assert.True(_partnerSaved, "A snackbar should be displayed when a partner has been saved");
        }

        [Test]
        public void ValidateSnackbarPartnerProvisioned()
        {
            Assert.True(_partnerProvisioned, "A snackbar should be displayed when a partner has been provisioned");
        }

        [Test]
        [Order(1)]
        public void ValidatePartnerIsListedOnPartnerPage()
        {
            var actualPartner =
                Go.To<PartnersPage>().FindPartner(expectedPartner);

            Assert.IsNotNull(actualPartner, "A partner should have been created with name" + expectedPartner.Name);

            _partnerListed = true;
        }

        [Test]
        [Order(2)]
        public void ValidatePartnerContactInfoOnPartnersPage()
        {
            if (!_partnerListed)
                Assert.Ignore("Partner not listed: " + expectedPartner.Name + " , test case cannot be executed");
            var actualPartner =
                Go.To<PartnersPage>().FindPartner(expectedPartner);
            new AssertObjectComparer<Partner>()
                .Compare(actualPartner, expectedPartner, new[] {nameof(Partner.ContactName)});
        }


        [Test]
        public void CannotCreatePartnerWithSameName()
        {
            if (!_partnerSaved)
                Assert.Inconclusive(AtataContext.Current.TestName +
                                    "is inconclusive as test case CreateAndProvisionPartnerAllRequiredFields did not succeed");
            Go.To<CreatePartnerPage>()
                .Create(expectedPartner)
                .Save<CreatePartnerPage>()
                .SnackBarPartnerAlreadyExists.Should.Exist();
        }


        [Test]
        [Order(2)]
        public void PartnerDetailsPageValidation()
        {
            if (!_partnerListed)
                Assert.Ignore("Partner not listed: " + expectedPartner.Name + " , test case cannot be executed");

            var actualPartnerOnDetailsPage =
                Go.To<PartnersPage>()
                    .SelectPartnerCard<PartnersDetailsPage>(expectedPartner)
                    .PartnerOnDetailsPage;
            new AssertObjectComparer<Partner>()
                .Compare(actualPartnerOnDetailsPage, expectedPartner);
        }


        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationSFTPConfigurationUserName()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList
                    .Single(x =>
                        x.Name.Contains("User Name") && x.Category.Equals("SFTPUsername"));
            Assert.True(configItem.Value.Contains(expectedPartner.Name.Replace(" ", "").ToLowerInvariant()),
                "Validating the Vale for User Name configuration Name includes the partner name");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationSFTPConfigurationPassword()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem = configurationsList
                .Single(x => x.Name.Equals("sFTP Password") && x.Category.Equals("SFTPPassword"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The password configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationLasoPGPPublicKey()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Equals("Laso Public Key") && x.Category.Equals("LasoPGPPublicKey"));
            Assert.True(configItem.Value.Contains("BEGIN PGP PUBLIC KEY BLOCK"),
                "Validating Laso Public Key was set to a value starting with -BEGIN PGP PUBLIC KEY BLOCK ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationLasoPGPPrivateKey()
        {
            var configurationsList =
                RetrievePartnerConfiguration();

            var configItem =
                configurationsList.Single(x =>
                    x.Name.Equals("Laso Private Key") && x.Category.Equals("LasoPGPPrivateKey"));
            Assert.True(configItem.Value.Contains("BEGIN PGP PRIVATE KEY BLOCK"),
                "Validating the LasoPGPPrivateKey was set to a value starting with -BEGIN PGP PRIVATE KEY BLOCK ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationLasoPGPPassphrase()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Equals("Laso PGP Pass Phrase") && x.Category.Equals("LasoPGPPassphrase"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The LasoPGPPassphrase configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationPublishedFileSystem()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("published Directory") && x.Category.Equals("PublishedFileSystemDirectory"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The published file system configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationRejectedFileSystemDirectory()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("rejected Directory") && x.Category.Equals("RejectedFileSystemDirectory"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The rejected directory configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationCuratedFileSystemDirectory()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("curated Directory") && x.Category.Equals("CuratedFileSystemDirectory"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The curated Directory configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationRawFileSystemDirectory()
        {
            var configurationsList =
                RetrievePartnerConfiguration();

            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("raw Directory") && x.Category.Equals("RawFileSystemDirectory"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The raw Directory configuration item should have a value ");
        }

        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationColdStorage()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("Cold Storage Container") && x.Category.Equals("ColdStorage"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The Cold Storage Container configuration item should have a value ");
        }


        [Test]
        [Order(2)]
        public void PartnerConfigurationValidationEscrowStorage()
        {
            var configurationsList =
                RetrievePartnerConfiguration();
            var configItem =
                configurationsList.Single(x =>
                    x.Name.Contains("Escrow Container") && x.Category.Equals("EscrowStorage"));
            Assert.False(string.IsNullOrEmpty(configItem.Value),
                "The Escrow Storage Container configuration item should have a value ");
        }

        private List<Configuration> RetrievePartnerConfiguration()
        {
            if (!_partnerListed)
                Assert.Ignore("Partner not listed: " + expectedPartner.Name + " , test case cannot be executed");

            var configurationsList =
                Go.To<PartnersPage>()
                    .SelectPartnerCard<PartnersDetailsPage>(expectedPartner)
                    .SelectViewPartnerConfiguration()
                    .ConfigurationOnPartnerConfigurationPage();
            Assert.True(configurationsList != null && configurationsList.Count > 1,
                "The configurations table should be populated");
            return configurationsList;
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
                    new Partner
                    {
                        ContactName = "Contact Name", ContactPhone = "512-2553633",
                        ContactEmail = "contact@partner.com", Name = ""
                    })
                .SetName("CreatePartnerRequiredFieldsNoPartnerName");
            yield return new TestCaseData(
                    new Partner
                    {
                        ContactName = "", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com",
                        Name = "Partner Name"
                    })
                .SetName("CreatePartnerRequiredFieldsNoContactName");
            yield return new TestCaseData(
                    new Partner
                    {
                        ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "",
                        Name = "Partner Name"
                    })
                .SetName("CreatePartnerRequiredFieldsNoContactEmail");
            yield return new TestCaseData(
                new Partner
                {
                    ContactName = "Contact Name", ContactPhone = "", ContactEmail = "contact@partner.com",
                    Name = "Partner Name"
                }).SetName("CreatePartnerRequiredFieldsNoContactPhone");
        }
    }
}