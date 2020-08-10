using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Insights.AccountTransactionClassifier.Function.Azure
{
    public interface IMachineLearningService
    {
        Task<MachineLearningSchema> GetSchema(CancellationToken cancellationToken);

        Task<MachineLearningExecutionObject> Execute(MachineLearningExecutionObject input, CancellationToken cancellationToken);
        Task<IEnumerable<MachineLearningExecutionObject>> Execute(IEnumerable<MachineLearningExecutionObject> inputs, CancellationToken cancellationToken);
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

    public class MachineLearningExecutionObject : Dictionary<string, IDictionary<string, object?>> { }
}
