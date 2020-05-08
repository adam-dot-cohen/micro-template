using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{
    [TestFixture]
    public class TestCsv : DataPipelineTests
    {/*
        [Test]
        public async Task TestDifferencesA()
        {
            AzureBlobStg za = new AzureBlobStg();
            string[] lines =
        await za.DownloadCsvFile(InsightsAutomationStorage.Key,
            InsightsAutomationStorage.Value, AutomationContainer, "dataquality/demographic/csv/validcsv/ValidCsv_AllLower_D_Demographic_20200304_20200304.csv");

            string[] lines2 =
                await za.DownloadCsvFile(InsightsAutomationStorage.Key,
                    InsightsAutomationStorage.Value, AutomationContainer, "dataquality/demographic/csv/validcsv/ValidCsv_AllUpper_D_Demographic_20200304_20200304.csv");

            var comparer = new ObjectsComparer.Comparer<string[]>();
  

            var isEqual = comparer.Compare(lines, lines2,
                out var differences);
            var dif = string.Join(Environment.NewLine, differences);
            dif = dif.Replace("Value1", "ActualCsv ").Replace("Value2", "Csv");
            Assert.True(isEqual,
                "The comparison between the expected and actual rows in the csv file resulted in differences " +
                dif);
 


        }*/

        [Test]
        public void   TestDifferencesB()
        {
            Task<StreamReader> v1 =testt2("dataquality/demographic/csv/validcsv/ValidCsv_AllLower_D_Demographic_20200304_20200304.csv");
            Task<StreamReader> v2 = testt2("dataquality/demographic/csv/validcsv/ValidCsv_AllUpper_D_Demographic_20200304_20200304.csv");
           List<string> difs= new CvsReadCompare().GetCompareStreams(v1.Result, v2.Result);
            Assert.True(difs.Count==0,"Expected files to be the same, but there are differences in the csv file "+ string.Join(",", difs));
        }

        public async Task<StreamReader> testt2(string file)
        {
            AzureBlobStg za = new AzureBlobStg();
            MemoryStream ms =
                await za.DownloadStream(InsightsAutomationStorage.Key,

                    InsightsAutomationStorage.Value, AutomationContainer, file);
                var cs = new CryptoStream(ms, new FromBase64Transform(), CryptoStreamMode.Write);
                var tr = new StreamWriter(cs);
            char? data=null;
                        tr.Write(data);
                        tr.Flush();
                        ms.Position = 0;
                        return new StreamReader(ms);
        }




  
    }
}
