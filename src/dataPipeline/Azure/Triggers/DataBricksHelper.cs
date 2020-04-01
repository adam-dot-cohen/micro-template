using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insights.Data.Triggers
{
    public static class DataBricksHelper
    {
        public class PythonJobRequest
        {
            public Int64 job_id;
            public List<string> python_params;
        }
        public class RunJobResult
        {
            public bool Success = false;
            public string Message;
            public RunNowResponse Response;
        }
        public class RunNowResponse
        {
            public Int64 run_id;
            public Int64 number_in_job;
        }
        private static string GetRunNowUri(string uriRoot)
        {
            return $"https://{uriRoot}.azuredatabricks.net/api/2.0/jobs/run-now";
        }

        public static async Task<RunJobResult> RunPythonJob(string uriRoot, string bearerToken, string payload, Int64 jobId)
        {
            var client = new HttpClient();
            RunJobResult result = null;

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            var body = JsonConvert.SerializeObject(new PythonJobRequest { job_id = jobId, python_params = new List<string>{payload} } );
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(GetRunNowUri(uriRoot), content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                result = new RunJobResult
                {
                    Success = true,
                    Message = "Success",
                    Response = JsonConvert.DeserializeObject<RunNowResponse>(responseBody)
                };                  
            }
            else
            {
                result = new RunJobResult{ Message = response.ReasonPhrase};
            }


            return result;
        }
        
    }
}
