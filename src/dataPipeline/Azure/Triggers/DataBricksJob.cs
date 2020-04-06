using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Insights.Data.Triggers
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

    public class DataBricksJob
    {
        private string _jobName;
        private ExecutionContext _context;
        private string _messageBody;
        private CancellationToken _token;
        private ILogger _log;

        private string _uriRoot;
        private string _bearerToken;
        private Int64 _jobId;

        public DataBricksJob(string jobName, ExecutionContext context, string messageBody, CancellationToken token, ILogger log)
        {
            _jobName = jobName;
            _context = context;
            _messageBody = messageBody;
            _token = token;
            _log = log;

            var config = new ConfigHelper(context).Config;
            _jobId = config.GetValue<Int64>($"jobId_{jobName}");
            _uriRoot = config.GetValue<string>("uriRoot");
            _bearerToken = config.GetValue<string>("bearerToken");
        }


        private string GetRunNowUri(string uriRoot)
        {
            return $"https://{_uriRoot}.azuredatabricks.net/api/2.0/jobs/run-now";
        }

//        public async Task<RunJobResult> RunPythonJob(string uriRoot, string bearerToken, string payload, Int64 jobId)
        public async Task<RunJobResult> RunPythonJob()
        {
            var client = new HttpClient();
            RunJobResult result = null;

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

            var body = JsonConvert.SerializeObject(new PythonJobRequest { job_id = _jobId, python_params = new List<string>{_messageBody} } );
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(GetRunNowUri(_uriRoot), content);

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

            if (result.Success == false)
            {
                var message = $"Failed to submit job: {result.Message}\n`tRequest Body:\n{body}";
                _log.LogError(message);
                throw new ApplicationException(message);
            }
            else
            {
                _log.LogInformation($"Job successfully submitted", result.Response);
            }

            return result;
        }
        
    }
}
