using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public class AzureMachineLearningService : IMachineLearningService
    {
        public Task<MachineLearningSchema> GetSchema(string baseUrl, string apiKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string GetRequest(MachineLearningExecutionObject input)
        {
            throw new NotImplementedException();
        }

        public string GetRequest(ICollection<MachineLearningExecutionObject> inputs)
        {
            throw new NotImplementedException();
        }

        public Task<MachineLearningExecutionObject> Execute(string baseUrl, string apiKey, MachineLearningExecutionObject input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<MachineLearningExecutionObject>> Execute(string baseUrl, string apiKey, ICollection<MachineLearningExecutionObject> inputs, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
