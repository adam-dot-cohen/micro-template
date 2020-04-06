using System.Threading;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core.Persistence
{
    public interface IBlobStorageService
    {
        Task CreateContainer(string name, CancellationToken cancellationToken);
        Task DeleteContainer(string containerName, CancellationToken cancellationToken);
        Task CreateDirectory(string containerName, string path, CancellationToken cancellationToken);
        Task UploadTextBlob(string containerName, string path, string text, CancellationToken cancellationToken);
    }

    //TODO: remove both of these before April 30th
    public interface IColdBlobStorageService : IBlobStorageService
    {
    }

    //TODO: remove both of these before April 30th
    public interface IEscrowBlobStorageService : IBlobStorageService
    {
    }
}