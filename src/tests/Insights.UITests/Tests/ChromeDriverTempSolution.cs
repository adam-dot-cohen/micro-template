using System;
using System.Net;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Insights.UITests.Tests
{
    /***
     * This is TEMPORARY hack to get a chrome driver that works for the version of chrome.
     * Works using the windows registry, gets the version of chrome installed : windows only
     * then goes to google api and matches the major version of the driver, extracting the chromedriver full version name.
     * The full version name is then used by WebDriverManager.
     * BEST SOLUTION: determine the browser versions that will be used for testing and package browser + driver + selenium 
     */
    public class ChromeDriverTempSolution
    {
        public void RequiredChromeDriver()
        {

            DriverManager wDriverManager = new DriverManager();
            string version =
             Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Google\Chrome\BLBeacon", "version", null).ToString();
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception("Chrome was not found in the registry");
            }

            string[] versionSp = version.Split(".");
            if (versionSp.Length < 2)
            {
                throw new Exception("cannot get version of chrome using the file version information");
            }
        
            String specV = GetChromeDriverRequiredVersionToDownload(versionSp[0]);
            if (String.IsNullOrEmpty(specV))
            {
                throw new Exception("cannot get a matching version of chromedriver to download for chrome version " + version);
            }

            wDriverManager.SetUpDriver(new ChromeConfig(), version: specV);

        }



        private string GetChromeDriverRequiredVersionToDownload(string version)
        {
            string chromedriverversion = "";
            using (var client = new WebClient())
            {

                var xml = client.DownloadString("https://chromedriver.storage.googleapis.com/");
                Thread.Sleep(10);
                XmlDocument s = new XmlDocument();
                s.LoadXml(xml);

                XmlNodeList xmlnodelist = s.ChildNodes[1].ChildNodes;

                for (int i = 0; i < xmlnodelist.Count; i++)

                {
                    if (
                        xmlnodelist[i].Name.Contains("Contents"))
                    {
                        XmlNodeList xmlnodelist3 = xmlnodelist[i].ChildNodes;

                        for (int i2 = 0; i2 < xmlnodelist3.Count; i2++)

                        {
                            if (xmlnodelist3[i2].Name.Contains("Key"))
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    xmlnodelist3[i2].Name + " " +
                                    xmlnodelist3[i2].InnerText);
                                if (xmlnodelist3[i2].InnerText.StartsWith(version))
                                {
                                    chromedriverversion = xmlnodelist3[i2].InnerText;
                                    chromedriverversion = chromedriverversion.Split("/")[0];
                                    break;
                                }
                            }
                        }

                    }

                    if (!String.IsNullOrEmpty(chromedriverversion))
                    {
                        break;

                    }
                }
            }

            return chromedriverversion;
        }

    }
}
