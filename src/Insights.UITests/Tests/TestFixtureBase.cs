using System;
using Atata;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Insights.UITests.Tests
{
    [TestFixture]
    public class TestFixtureBase
    {


        [SetUp]
        public void SetUp()
        {
            AtataContext.Configure().
                UseChrome().
                //.WithArguments("start-maximized", "disable-infobars", "disable-extensions").
                UseBaseUrl("https://localhost:5001").
                UseNUnitTestName().
                AddNUnitTestContextLogging().
                AddScreenshotFileSaving().
                // Below are possible ways to specify folder path to store screenshots for individual tests.
                // Both examples build the same path which is used by default.
                //    WithFolderPath(@"Logs\{build-start}\{test-name}").
                //    WithFolderPath(() => $@"Logs\{AtataContext.BuildStart:yyyy-MM-dd HH_mm_ss}\{AtataContext.Current.TestName}").
                LogNUnitError().
                TakeScreenshotOnNUnitError().
                UseAssertionExceptionType<NUnit.Framework.AssertionException>().
                UseNUnitAggregateAssertionStrategy().
                UseAllNUnitFeatures().
                Build();
        }

        [TearDown]
        public void TearDown()
        {
            TestStatus testStatus =
                TestContext.CurrentContext.Result.Outcome.Status;
            Console.WriteLine(testStatus.ToString());
            Console.WriteLine(AtataContext.Current.Log);
            if (AtataContext.Current != null)
                AtataContext.Current.CleanUp();
        }

    }
}
