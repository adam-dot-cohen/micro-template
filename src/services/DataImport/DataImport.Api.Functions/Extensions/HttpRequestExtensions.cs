using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Api.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace DataImport.Api.Extensions
{
    internal static class HttpRequestExtensions
    {
        public static async Task<HttpRequestBody<T>> GetModelAsync<T>(this HttpRequest req)
        {
            var body = await req.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<T>(body);

            var result = new HttpRequestBody<T>();
            var valResult = new List<ValidationResult>();

            // one flaw here; it does not recursively validate the object graph
            result.Model = model;
            result.IsValid = Validator.TryValidateObject(model, new ValidationContext(model), valResult, true);
            result.ValidationMessages = valResult.Select(r => r.ErrorMessage);

            return result;
        }
    }
}
