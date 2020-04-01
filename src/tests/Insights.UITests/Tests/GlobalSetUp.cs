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
                .ApplyJsonConfig()
                .UseBaseUrl(InsightsManagerUrl)
                .AddScreenshotFileSaving()
                .WithFolderPath(@"Logs\")
                .WithFileName("{test-name}"); // Below are possible ways to specify folder path to store screenshots for individual tests.
             // Both examples build the same path which is used by default.
             //    WithFolderPath(@"Logs\{build-start}\{test-name}").
             //    WithFolderPath(() => $@"Logs\{AtataContext.BuildStart:yyyy-MM-dd HH_mm_ss}\{AtataContext.Current.TestName}").
        }

        private void ResolveEnvironment()
        {

            InsightsManagerUrl = TestContext.Parameters.Get("InsightsManagerUrl");
            IdentityUrl = TestContext.Parameters.Get("IdentityUrl");

            if (string.IsNullOrEmpty(InsightsManagerUrl))
            {
                //this is just defaulting, because there is no way to pass parameters to nunit with dotnet test
                string environment = TestContext.Parameters.Get("Environment", "local");

                JObject envJObject = JObject.Parse(File.ReadAllText(
                    Path.Combine(Directory.GetCurrentDirectory()) + "/EnvironmentConfigurations/" + environment + ".json"));

                InsightsManagerUrl = envJObject.GetValue("InsightsManagerUrl").ToString();
                IdentityUrl = envJObject.GetValue("IdentityUrl").ToString();

            }

            if (string.IsNullOrEmpty(InsightsManagerUrl) && string.IsNullOrEmpty(IdentityUrl))
            {
                throw new Exception("Urls for the application were not set.");
            }
        }


    }
}
