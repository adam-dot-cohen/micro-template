using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Insights.Data.Triggers.Services
{
    public class PythonJobRequest
    {
        public long job_id;
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
        public long run_id;
        public long number_in_job;
    }

    public interface IDataBricksJobService
    {
        Task<RunJobResult> RunPythonJob(string messageBody, JobState jobState, CancellationToken cancellationToken);
    }

    public class DataBricksJobService : IDataBricksJobService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataBricksJobService> _log;
        private const string RunNowUri = "/api/2.0/jobs/run-now";

        public DataBricksJobService(HttpClient httpClient, ILogger<DataBricksJobService> log)
        {
            _httpClient = httpClient;
            _log = log;
        }

        public async Task<RunJobResult> RunPythonJob(string messageBody, JobState jobState, CancellationToken cancellationToken)
        {
            RunJobResult result;

            var body = JsonConvert.SerializeObject(new PythonJobRequest { job_id = long.Parse(jobState.Latest.JobId), python_params = new List<string>{messageBody} } );
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(RunNowUri, content, cancellationToken);

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
