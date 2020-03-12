using System;
using Atata;
using Insights.UITests.TestData.Identity.Users;
using Insights.UITests.UIComponents.AdminPortal.Pages.Login;
using NUnit.Framework;

namespace Insights.UITests.Tests
{
    [TestFixture]
    public class TestFixtureBase
    {
        [SetUp]
        public void SetUp()
        {
            AtataContext.Configure()
                .UseChrome().
                //.WithArguments("start-maximized", "disable-infobars", "disable-extensions").
                UseBaseUrl("https://localhost:5001").UseNUnitTestName().AddNUnitTestContextLogging()
                .AddScreenshotFileSaving().
                // Below are possible ways to specify folder path to store screenshots for individual tests.
                // Both examples build the same path which is used by default.
                //    WithFolderPath(@"Logs\{build-start}\{test-name}").
                //    WithFolderPath(() => $@"Logs\{AtataContext.BuildStart:yyyy-MM-dd HH_mm_ss}\{AtataContext.Current.TestName}").
                LogNUnitError().TakeScreenshotOnNUnitError()
                .UseAssertionExceptionType<NUnit.Framework.AssertionException>().UseNUnitAggregateAssertionStrategy()
                .UseAllNUnitFeatures().Build();
        }

        [SetUp]
        public void Login()
        {
            InsightsManagerUser insightsManagerUser = new InsightsManagerUser { Username = "ollie@laso.com", Password = "ollie" };
            Go.To<LoginPage>().SetForm(insightsManagerUser)
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