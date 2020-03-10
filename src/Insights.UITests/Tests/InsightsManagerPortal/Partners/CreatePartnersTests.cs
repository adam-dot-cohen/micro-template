using System;
using System.Collections.Generic;
using System.Linq;
using Atata;
using Insights.UITests.TestData.Partners;
using Insights.UITests.UIComponents.AdminPortal.Pages.Partners;
using NUnit.Framework;

namespace Insights.UITests.Tests.InsightsManagerPortal.Partners
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("PartnersTable")]
    public class CreatePartnersTests : TestFixtureBase
    {
        Partner expectedPartner = new Partner { ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com", Name = Randomizer.GetString("PartnerName{0}", 12) };
        
        [Test, Order(1)]
        public void CreatePartnerAllRequiredFields()
        { 
            
           List<Partner> actualPartnerList =
                Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<PartnersPage>()
                    .SnackBarPartnerSaved(expectedPartner.Name).Should.BeVisible()
                    .SnackBarPartnerSaved(expectedPartner.Name).WaitTo.Not.BeVisible()
                    .PartnersList
                    .FindAll(x => x.Name.Equals(expectedPartner.Name));
            
           Assert.True(actualPartnerList.Count==1, "A partner should have been created with name" + expectedPartner.Name);

            var comparer = new ObjectsComparer.Comparer<Partner>();
            comparer.IgnoreMember(nameof(Partner.ContactName));
            
            bool isEqual = comparer.Compare(expectedPartner, actualPartnerList[0],
                out var differences);
            string dif = string.Join(Environment.NewLine, differences);
            dif = dif.Replace("Value1", "Expected").Replace("Value2", "Actual");
            Assert.True(isEqual,
                "The comparison between the expected and actual values for the partner data resulted in differences " +
                dif);
        }

        
        [Test]
        public void CannotCreatePartnerWithSameName()
        {

            Go.To<CreatePartnerPage>()
                    .Create(expectedPartner)
                    .Save<CreatePartnerPage>()
                    .SnackBarPartnerAlreadyExists.Should.Exist();
        }


        [Test]
        public void PartnerDetailsValidation()
        {
            Partner expectedPartner = new Partner { ContactName = "Contact Name", ContactPhone = "512-2553633", ContactEmail = "contact@partner.com", Name = "PartnerNamenfiagchmjnbi" };

            PartnersDetailsPage.MatCard matCard =
            Go.To<PartnersPage>()
                .FindPartner(expectedPartner).ContactName.ClickAndGo<PartnersDetailsPage>()
                .PartnerDetails.Count.Should.Equal(1)
                .PartnerDetails.FirstOrDefault();
            Assert.NotNull(matCard);
            Console.WriteLine(matCard.Name.Value);
            Console.WriteLine(matCard.Email.Value);
            Console.WriteLine(matCard.Phone.Value);
            Assert.Multiple(()=>{
                Assert.True(matCard.Name.Attributes.GetValue("ng-reflect-value").Equals(expectedPartner.ContactName));
                Assert.True(matCard.Email.Attributes.GetValue("ng-reflect-value").Equals(expectedPartner.ContactEmail));
                Assert.True(matCard.Phone.Attributes.GetValue("ng-reflect-value").Equals(expectedPartner.ContactPhone));
            }
            );

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

            //4 iterations: FOR ERRORS ON SCREENSHOT TO BE CAPTURED INDIVIDUALLY NEED TO SET THE TEST CASE NAME TO SOMETHING MEANINGFUL AND DIFFERENT THAN THE DATA DRIVEN TEST CASE
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

        [Test]
        public void tr()
        {
            IEnumerator<PartnersDetailsPage.MatCard> e =
            Go.To<PartnersDetailsPage>(url: "partners/detail/078e960e-cf53-4572-bb7f-026d5d13e7c9")
                .PartnerDetails.GetEnumerator();
 
            
            while (e.MoveNext())
            {
                if (e.Current != null)
                {
                    Console.WriteLine("This is the email "+e.Current.Email.Attributes.GetValue("ng-reflect-value"));
                    
                }
                 
            }

        }




    }
}
