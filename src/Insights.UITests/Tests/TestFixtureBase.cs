using System;
using Atata;
using Insights.UITests.TestData.Identity.Users;
using Insights.UITests.UIComponents.AdminPortal.Pages.Login;
using NUnit.Framework;

namespace Insights.UITests.Tests
{
    [TestFixture]
    public abstract class TestFixtureBase
    {

        [SetUp]
        public void Login()
        {
            AtataContext.Configure().UseDriver(DriverAliases.Chrome).Build();
            InsightsManagerUser insightsManagerUser = new InsightsManagerUser { Username = "ollie@laso.com", Password = "ollie" };
            Go.To<LoginPage>(url: GlobalSetUp.IdentityUrl).SetForm(insightsManagerUser)
                ._SaveForm();

            Go.To<LoginPage>()
                .UserLink.Attributes.TextContent.Should.EqualIgnoringCase(insightsManagerUser.Username);

        }

        [TearDown]
        public void TearDown()
        {
            var testStatus =
                TestContext.CurrentContext.Result.Outcome.Status;
            Console.WriteLine(testStatus.ToString());
            Console.WriteLine(AtataContext.Current.Log);
            if (AtataContext.Current != null)
                AtataContext.Current.CleanUp();
        }
    }
}