using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public interface IMachineLearningService
    {
        Task<MachineLearningSchema> GetSchema(string baseUrl, string apiKey, CancellationToken cancellationToken);

        string GetRequest(MachineLearningExecutionObject input);
        string GetRequest(ICollection<MachineLearningExecutionObject> inputs);

        Task<MachineLearningExecutionObject> Execute(string baseUrl, string apiKey, MachineLearningExecutionObject input, CancellationToken cancellationToken);
        Task<ICollection<MachineLearningExecutionObject>> Execute(string baseUrl, string apiKey, ICollection<MachineLearningExecutionObject> inputs, CancellationToken cancellationToken);
    }

    public class MachineLearningSchema
    {
        public MachineLearningSchemaObject[] Inputs { get; set; } = null!;
        public MachineLearningSchemaObject[] Outputs { get; set; } = null!;
    }

    public class MachineLearningSchemaObject
    {
        public string Name { get; set; } = null!;
        public MachineLearningSchemaObjectColumn[] Columns { get; set; } = null!;
    }

    public class MachineLearningSchemaObjectColumn
    {
        public string Name { get; set; } = null!;
        public Type Type { get; set; } = null!;
    }

    public class MachineLearningExecutionObject : Dictionary<string, IDictionary<string, object>> { }
}
