using System;
using System.Collections.Generic;
using Atata;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using Laso.Identity.Domain.Entities;
using NUnit.Framework;

namespace Insights.UITests.Tests.AdminPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("PartnersTable")]
    public class CreatePartnersTests : TestFixtureBase
    {

        [Test]
        public void NoPartnersInTable()
        {
            Assert.AreEqual(0,
                Go.To<PartnersPage>().PartnerList.Count, "Empty table");
        }

        [Test]
        public void CreatePartnerAllRequiredData()
        {
            Go.To<CreatePartnerPage>().PartnerName.Set("FI Partner Name2")
                    .PrimaryContactName.Set("Ollie Partner2")
                    .PrimaryContactEmail.Set("ollie2@lasoinsights.com")
                    .PrimaryContactPhone.Set("512-255-3660")
                    .Save.Should.BeEnabled()
                    .Save.ClickAndGo<PartnersPage>().
                    PartnersTable.Rows[x =>
                        x.PartnerName == "FI Partner Name2"
                        && x.ContactName == "Ollie Partner2"
                        && x.ContactEmail == "ollie2@lasoinsights.com"
                        && x.ContactPhone == "512-255-3660"].Should.Exist();

        }

        [Test]
        public void CreatePartnerRequiredFieldsUsingEntityData()
        {
            Partner partner = new Partner { ContactName = "t", ContactPhone = "t", ContactEmail = "t", Name = "t" };
            Go.To<CreatePartnerPage>()
                .SetPartnerEntityTestObject(partner)
                .Create<PartnersPage>()
                .Wait(2)
                .PartnersTable.Rows[x =>
                x.PartnerName == partner.Name
                && x.ContactName == partner.ContactName
                && x.ContactEmail == partner.ContactEmail
                && x.ContactPhone == partner.ContactPhone].Should.Exist();

        }

        [Test]
        public void CreatePartnerRequiredFieldsUsingEntityDataAndTableData()
        {
            Partner expectedPartner = new Partner { ContactName = "t", ContactPhone = "t", ContactEmail = "t", Name = "t" };
            Partner actualPartner =
            Go.To<CreatePartnerPage>()
                .SetPartnerEntityTestObject(expectedPartner)
                .Create<PartnersPage>()
                .PartnerList.Find(x=>x.Name.Equals(expectedPartner.Name));
            Assert.NotNull(actualPartner,"A partner should have been created with name"+expectedPartner.Name); 
            
            var comparer = new ObjectsComparer.Comparer<Partner>();


            comparer.IgnoreMember(nameof(actualPartner.PublicKey));
            comparer.IgnoreMember(nameof(actualPartner.PartitionKey));
            comparer.IgnoreMember(nameof(actualPartner.Id));


            bool isEqual = comparer.Compare(actualPartner, expectedPartner,
                out var differences);
            string dif = string.Join(Environment.NewLine, differences);
            dif = dif.Replace("Value1", "Expected").Replace("Value2", "Actual");
            Assert.True(isEqual,
                "The comparison between the expected and actual values for the partner data resulted in differences " +
                dif);
        }



        [Test]
        [TestCaseSource(nameof(PartnerEntityTestDataObjects))]
        public void CreatePartnerRequiredFields(Partner partner)
        {
            Console.WriteLine(TestContext.CurrentContext.Test.Name);
            Go.To<CreatePartnerPage>()
                .SetPartnerEntityTestObject(partner)
                .CreateWithTestEntityTestObject()
                .Wait(2)
                .Save.Should.BeDisabled(); 
           
        }

        public static IEnumerable<TestCaseData> PartnerEntityTestDataObjects()
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
                .SetName("CreatePartnerRequiredFieldsNoContactPhone");
            

        }

        [Test]
        public void testTable()
        {

            Partner partner = new Partner { ContactName = "l", ContactPhone = "t", ContactEmail = "t", Name = "t" };
            Go.To<PartnersPage>().
                PartnersTable.Rows[x => 
                    x.PartnerName == partner.Name 
                    && x.ContactName == partner.ContactName 
                    && x.ContactEmail == partner.ContactEmail 
                    && x.ContactPhone == partner.ContactPhone].Should.Exist();
        }




    }
}
