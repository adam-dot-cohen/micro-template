using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Atata;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Insights.UITests.Tests
{
    [SetUpFixture]
    public class GlobalSetUp
    {

        public static string InsightsManagerUrl = "";
        public static string IdentityUrl = "";
        string environment = "";

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

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            StopLocalInsights();
        }


        private void ResolveEnvironment()
        {
            
            InsightsManagerUrl = TestContext.Parameters.Get("InsightsManagerUrl");
            IdentityUrl = TestContext.Parameters.Get("IdentityUrl");

            if (string.IsNullOrEmpty(InsightsManagerUrl))
            {
                //this is just defaulting, because there is no way to pass parameters to nunit with dotnet test
                environment = TestContext.Parameters.Get("Environment", "local");

                JObject envJObject = JObject.Parse(File.ReadAllText(
                    Path.Combine(Directory.GetCurrentDirectory()) + "/EnvironmentConfigurations/" + environment + ".json"));

                InsightsManagerUrl = envJObject.GetValue("InsightsManagerUrl").ToString();
                IdentityUrl = envJObject.GetValue("IdentityUrl").ToString();

            }

            if(String.Equals(environment, "local", StringComparison.OrdinalIgnoreCase))
            { 
                
                 StartInsightsLocally();
            }

            if (string.IsNullOrEmpty(InsightsManagerUrl) && string.IsNullOrEmpty(IdentityUrl))
            {
                throw new Exception("Urls for the application were not set.");
            }
        }

        private void StartInsightsLocally()
        {
            string startupPath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;

            Runspace r = RunspaceFactory.CreateRunspace();
            var powerShell = PowerShell.Create();
            powerShell.Runspace = r;
            r.Open();
            r.SessionStateProxy.Path.SetLocation(startupPath);
            powerShell.AddScript(startupPath + "\\solutionscripts\\start.ps1");
            powerShell.AddCommand("Set-ExecutionPolicy").AddArgument("Unrestricted");
            powerShell.Invoke();
                using (Pipeline pipeline = r.CreatePipeline())
            {
                pipeline.Commands.Add("Start-Insights");
                pipeline.Invoke();
            }
            r.Close();
            
        }

        private void StopLocalInsights()
        {
            if (String.Equals(environment, "local", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Process.GetProcesses()
                    .Where(x => x.ProcessName.ToLower()
                        .StartsWith("laso")||x.MainWindowTitle.ToLower().StartsWith("ng serve"))
                    .ToList()
                    .ForEach(x => x.Kill());
            }
        }


    }
}
