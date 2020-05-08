using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Laso
{
    [SetUpFixture]
    [TestFixture]
    public class GlobalSetUp
    {
        public static KeyValuePair<string, string> ColdStorage;
        public static KeyValuePair<string, string> EscrowStorage;
        public static KeyValuePair<string, string> MainInsightsStorage;
        public static KeyValuePair<string, string> InsightsAutomationStorage;
        public static string ApiClientUrl;
        public static string ApiClientHost;
        public static string ApiClientCookie = "";
        public static string AutomationPartnerId;
        public static string AutomationPartnerName;
        public static string AutomationContainer;

        static GlobalSetUp()
        {
            ResolveEnvironment();
        }
 

        private static void ResolveEnvironment()
        {
            //future the default will be local, right now only develop is ready
            var environment = TestContext.Parameters.Get("Environment", "develop");

            var envJObject = JObject.Parse(File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory()) + "/dataPipeline." + environment + ".json"));
            ApiClientCookie = (string) envJObject["Api"]["cookie"]; 
            ApiClientUrl = (string) envJObject["Api"]["url"]; 
            ApiClientHost = (string) envJObject["Api"]["host"]; 
            AutomationPartnerId =
                (string) envJObject[
                    "AutomationPartner"]["id"];
            AutomationPartnerName =
                (string)envJObject[
                    "AutomationPartner"]["name"];
            AutomationContainer =
                (string)envJObject[
                    "AutomationStorage"]["Container"];

            ColdStorage = new KeyValuePair<string, string>((string) envJObject["ColdStorage"]["Account"],
                (string) envJObject["ColdStorage"][
                    "Key"]); 
            EscrowStorage = new KeyValuePair<string, string>((string) envJObject["EscrowStorage"]["Account"],
                (string) envJObject["EscrowStorage"][
                    "Key"]); 
            MainInsightsStorage = new KeyValuePair<string, string>((string) envJObject["MainInsightStorage"]["Account"],
                (string) envJObject[
                    "MainInsightStorage"][
                    "Key"]);
            InsightsAutomationStorage = new KeyValuePair<string, string>((string)envJObject["AutomationStorage"]["Account"],
                (string)envJObject[
                    "AutomationStorage"][
                    "Key"]);
             
        }
    }
}