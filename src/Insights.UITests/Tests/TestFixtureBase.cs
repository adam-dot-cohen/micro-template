using System;
using Atata;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Insights.UITests.Tests
{
    [TestFixture]
    public class TextFixtureBase
    {


        [SetUp]
        public void SetUp()
        {
            AtataContext.Configure().
                UseChrome().
                    WithArguments("start-maximized", "disable-infobars", "disable-extensions").
                // Base URL can be set here, but in this sample it is defined in Atata.json config file.
                //UseBaseUrl("https://demo.atata.io/").
                UseCulture("en-US").
            UseNUnitTestName().
                AddNUnitTestContextLogging().
                // Configure logging:
                //    WithMinLevel(LogLevel.Info).
                //    WithoutSectionFinish().
                AddNLogLogging().// Actual NLog configuration is located in NLog.config file.
                                 // Logs can be found in "{repo folder}\src\AtataSampleApp.UITests\bin\Debug\Logs\"
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
