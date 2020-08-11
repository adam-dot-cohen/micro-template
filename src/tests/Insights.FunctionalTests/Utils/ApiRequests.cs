using System;
using System.IO;
using System.Linq;
using System.Net;
using Laso.AdminPortal.Core.DataRouter.Queries;
using Newtonsoft.Json;

namespace Laso.Insights.FunctionalTests.Utils
{
    public class ApiRequests
    {
        public FileBatchViewModel GetFileBatchViewModel(string fileName, string partnerId)
        {
            var responseStr = string.Empty;
            var url = GlobalSetUp.TestConfiguration.Api.Url + "/partners/" + partnerId + "/analysishistory";

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Accept =
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            request.Host = GlobalSetUp.TestConfiguration.Api.Host;
            request.Headers.Add("cookie", GlobalSetUp.TestConfiguration.Api.Cookie ?? "");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            var response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Analysis History Request failed " + response.StatusCode);
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            {
                responseStr = reader.ReadToEnd();
                response.Close();
                stream.Close();
            }
            var partnerHistory =
                JsonConvert.DeserializeObject<PartnerAnalysisHistoryViewModel>(responseStr);
            Console.WriteLine(partnerHistory.PartnerId);

            var fileBatches = partnerHistory.FileBatches.ToList();
            var fileBatchViewModel =
                fileBatches.Find(x =>
                    x.Files.Any(r => r.Filename.Equals(fileName)));

            return
                fileBatchViewModel;
        }
    }
}