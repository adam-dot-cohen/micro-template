using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Atata;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Insights.UITests.Tests
{
    [SetUpFixture]
    public class GlobalSetUp
    {

        public static string InsightsManagerUrl = "";
        public static string IdentityUrl = "";


        [OneTimeSetUp]
        public void SetUp()
        {
            ResolveEnvironment();
            new ChromeDriverTempSolution().RequiredChromeDriver();
            AtataContext.GlobalConfiguration
                .UseChrome()
                .WithArguments("start-maximized", "disable-infobars", "disable-extensions", "headless")
                .UseBaseUrl(InsightsManagerUrl)
                .UseNUnitTestName().AddNUnitTestContextLogging()
                .AddScreenshotFileSaving().
                // Below are possible ways to specify folder path to store screenshots for individual tests.
                // Both examples build the same path which is used by default.
                //    WithFolderPath(@"Logs\{build-start}\{test-name}").
                //    WithFolderPath(() => $@"Logs\{AtataContext.BuildStart:yyyy-MM-dd HH_mm_ss}\{AtataContext.Current.TestName}").
                LogNUnitError().TakeScreenshotOnNUnitError()
                .UseAssertionExceptionType<NUnit.Framework.AssertionException>().UseNUnitAggregateAssertionStrategy()
                .UseAllNUnitFeatures();
        }

        private void ResolveEnvironment()
        {
            string environment = TestContext.Parameters.Get("Environment", "local");

            JObject envJObject = JObject.Parse(File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory()) + "/EnvironmentConfigurations/" + environment + ".json"));

            InsightsManagerUrl = envJObject.GetValue("InsightsManagerUrl").ToString();
            IdentityUrl = envJObject.GetValue("IdentityUrl").ToString();
        }

 
    }
}
