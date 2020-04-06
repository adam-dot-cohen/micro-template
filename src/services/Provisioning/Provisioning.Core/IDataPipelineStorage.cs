using System.Threading;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface IDataPipelineStorage
    {
        Task CreateFileSystem(string fileSystemName, CancellationToken cancellationToken);
        Task CreateDirectory(string fileSystemName, string directoryName, CancellationToken cancellationToken);
    }
}
