using System;
using System.Collections.Generic;
using Atata;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using Laso.AdminPortal.Web.Api.Partners;
using Laso.Identity.Domain.Entities;
using Laso.Logging.Extensions;
using NUnit.Framework;

namespace Insights.UITests.Tests.AdminPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("PartnersTable")]
    public class CreatePartnersTests : TestFixtureBase
    {


        [Test]
        public void CreatePartnerRequiredFieldsUsingEntityData()
        {
            PartnerViewModel partner = new PartnerViewModel { ContactName = "t", ContactPhone = "t", ContactEmail = "t", Name = "t" };

            List<PartnerViewModel> actualPartner =
            Go.To<CreatePartnerPage>()
                .Create(partner)
                .Save<PartnersPage>()
                .Wait(2)
                .PartnerList.FindAll(x =>
                    x.Name == partner.Name
                    && x.ContactName == partner.ContactName
                    && x.ContactEmail == partner.ContactEmail
                    && x.ContactPhone == partner.ContactPhone);

            Assert.True(actualPartner.IsNotNullOrEmpty() && actualPartner.Count ==1,"Partner "+actualPartner +" should be found on the partners table");
            
        }

        [Test]
        public void CreatePartnerRequiredFieldsUsingEntityDataAndTableData()
        {
            PartnerViewModel expectedPartner = new PartnerViewModel { ContactName = "t", ContactPhone = "t", ContactEmail = "t", Name = "t" };
            PartnerViewModel actualPartner =
            Go.To<CreatePartnerPage>()
                .Create(expectedPartner)
                .Save<PartnersPage>()
                .PartnerList.Find(x=>x.Name.Equals(expectedPartner.Name));
            Assert.NotNull(actualPartner,"A partner should have been created with name"+expectedPartner.Name); 
            
            var comparer = new ObjectsComparer.Comparer<PartnerViewModel>();
            comparer.IgnoreMember(nameof(actualPartner.PublicKey));
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
        [TestCaseSource(nameof(PartnerTestDataObjects))]
        public void CreatePartnerRequiredFields(PartnerViewModel partner)
        {
            Console.WriteLine(TestContext.CurrentContext.Test.Name);
            PartnerViewModel expectedPartner = new PartnerViewModel { ContactName = "t", ContactPhone = "t", ContactEmail = "t", Name = "t" };

            Go.To<CreatePartnerPage>()
                .Create(expectedPartner)
                .Wait(2)
                .SaveButton.Should.BeEnabled(); 
           
        }

        public static IEnumerable<TestCaseData> PartnerTestDataObjects()
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
        public void TestTable()
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
