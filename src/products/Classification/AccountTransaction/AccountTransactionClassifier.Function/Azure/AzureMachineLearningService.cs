using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Extensions;
using Newtonsoft.Json.Linq;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public class AzureMachineLearningService : IMachineLearningService
    {
        private readonly IRetryPolicy _retryPolicy;

        public AzureMachineLearningService(IRetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
        }

        public string BaseUrl { get; set; } = null!;
        public string ApiKey { get; set; } = null!;

        public async Task<MachineLearningSchema> GetSchema(CancellationToken cancellationToken)
        {
            var response = await GetAsync("swagger.json", cancellationToken);

            var responseJson = JObject.Parse(response);

            return new MachineLearningSchema
            {
                Inputs = GetSchemaObjects(responseJson, "definitions.ExecutionInputs"),
                Outputs = GetSchemaObjects(responseJson, "definitions.ExecutionOutputs")
            };
        }
        public async Task<MachineLearningExecutionObject> Execute(MachineLearningExecutionObject input, CancellationToken cancellationToken)
        {
            var result = await Execute(new[] { input }, cancellationToken);
            return result.Single();
        }

        public Task<IEnumerable<MachineLearningExecutionObject>> Execute(IEnumerable<MachineLearningExecutionObject> inputs, CancellationToken cancellationToken)
        {
            var request = GetRequest(inputs);

            // TODO: narrow down which exceptions to retry?
            return _retryPolicy.RetryOnException<Exception, Task<IEnumerable<MachineLearningExecutionObject>>>(
                5, 
                retryAttempt => TimeSpan.FromSeconds(DelayCalculator.ExponentialDelay(retryAttempt)), 
                async () =>
            {
                var response = await PostAsync("execute?api-version=2.0&details=true", request, cancellationToken);

                var responseJson = JObject.Parse(response);

                if (responseJson["error"] != null)
                {
                    throw new AzureMachineLearningException(
                        responseJson["error"]["code"].Value<string>(),
                        responseJson["error"]["message"].Value<string>(),
                        responseJson["error"]["details"].Value<JArray>()
                            .Children<JObject>()
                            .Select(x => new AzureMlErrorDetail
                            {
                                Code = x["code"].Value<string>(),
                                Message = x["message"].Value<string>()
                            })
                            .ToArray());
                }

                var results = new List<MachineLearningExecutionObject>();

                responseJson["Results"]
                    .Children<JProperty>()
                    .ForEach(x =>
                    {
                        var inputName = x.Name;
                        var columns = x.Value["value"]["ColumnNames"].ToObject<string[]>()
                            .Zip(x.Value["value"]["ColumnTypes"].ToObject<string[]>(), (y, z) => new { Name = y, Type = z })
                            .ToList();

                        x.Value["value"]["Values"].Children<JArray>()
                            .ForEach((y, i) =>
                            {
                                var values = y.ToObject<JValue[]>();

                                if (results.Count <= i)
                                    results.Add(new MachineLearningExecutionObject());

                                results[i][inputName] = columns
                                    .Zip(values, (column, value) => new { column.Name, Value = GetValue(column.Type, value) })
                                    .ToDictionary(z => z.Name, z => (object?)z.Value);
                            });
                    });

                return results.AsEnumerable();
            });
        }

        public string GetRequest(IEnumerable<MachineLearningExecutionObject> inputs)
        {
            var inputList = inputs.ToList();

            // TODO: enforce all inputs have same schema
            var inputSchema = inputList.First()
                .Select(x => new { InputName = x.Key, ColumnNames = x.Value.Keys })
                .ToList();

            var requestJson = new JObject(
                new JProperty("Inputs", new JObject(inputSchema.Select(x => new JProperty(x.InputName, new JObject(
                    new JProperty("ColumnNames", new JArray(x.ColumnNames)),
                    new JProperty("Values", new JArray(inputList.Select(y =>
                    {
                        var inputObject = y[x.InputName];
                        return (object)new JArray(x.ColumnNames.Select(z => inputObject[z]).ToArray());
                    }).ToArray()))))))),
                new JProperty("GlobalParameters", new JObject()));

            return requestJson.ToString();
        }
        
        private static MachineLearningSchemaObject[] GetSchemaObjects(JToken token, string path)
        {
            return token.SelectToken(path + ".properties")
                .Children<JProperty>()
                .Select(x =>
                {
                    var propertyType = x.Value["type"].Value<string>();

                    if (propertyType != "array")
                        throw new NotSupportedException("Unsupported definition: " + propertyType);

                    var reference = x.Value["items"]["$ref"].Value<string>();

                    const string refObjectPrefix = "#/definitions/";

                    if (!reference.StartsWith(refObjectPrefix))
                        throw new NotSupportedException("Unsupported schema object reference: " + reference);

                    return new MachineLearningSchemaObject
                    {
                        Name = x.Name,
                        Columns = token.SelectToken("definitions." + reference.Substring(refObjectPrefix.Length) + ".properties")
                            .Children<JProperty>()
                            .Select(y => new MachineLearningSchemaObjectColumn
                            {
                                Name = y.Name,
                                Type = GetType(y.Value["type"].Value<string>())
                            })
                            .ToArray()
                    };
                })
                .ToArray();
        }
        
        private static Type GetType(string type)
        {
            return type switch
            {
                "string" => typeof(string),
                "number" => typeof(double),
                "integer" => typeof(int),
                "boolean" => typeof(bool),
                _ => throw new NotSupportedException("Unsupported column type: " + type)
            };
        }
        
        private static object GetValue(string type, JValue value)
        {
            return type.ToLower() switch
            {
                "string" => value.Value<string>(),
                "float" => value.Value<double>(),
                "double" => value.Value<double>(),
                "integer" => value.Value<int>(),
                "int32" => value.Value<int>(),
                "int64" => value.Value<long>(),
                "boolean" => value.Value<bool>(),
                _ => throw new NotSupportedException("Unsupported column type: " + type)
            };
        }

        private async Task<string> GetAsync(string resource, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            
            var request = new HttpRequestMessage(HttpMethod.Get, resource);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        private async Task<string> PostAsync(string resource, string value, CancellationToken cancellationToken)
        {
            var baseUri = new Uri(BaseUrl);
            var httpClient = new HttpClient { BaseAddress = baseUri };

            var content = new StringContent(value, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, resource) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }

    public class AzureMachineLearningException : Exception
    {
        public string Code { get; set; }
        public AzureMlErrorDetail[] Details { get; set; }

        public AzureMachineLearningException(string code, string message, AzureMlErrorDetail[] details) :
            base(string.Join("\r\n", new[] { message }.Concat(details.Select(d => d.Message))))
        {
            Code = code;
            Details = details;
        }
    }

    public class AzureMlErrorDetail
    {
        public string? Code { get; set; }
        public string? Target { get; set; }
        public string? Message { get; set; }
    }
}
