using System.Threading;
using System.Threading.Tasks;

namespace Insights.AccountTransactionClassifier.Function
{
    public interface IAccountTransactionClassifyBatchProcess
    {
        Task Run(string partnerId, string inputFilename, string outputFilename, CancellationToken cancellationToken);
    }
}