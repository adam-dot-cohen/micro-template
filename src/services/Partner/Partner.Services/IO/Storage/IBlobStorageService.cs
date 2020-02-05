using System.IO;

namespace Partner.Services.IO.Storage
{
    public interface IReadOnlyBlobStorageService
    {
        bool Exists(string container, string blobName);

        Stream OpenRead(string container, string blobName);
    }

    public interface IBlobStorageService : IReadOnlyBlobStorageService
    {
        Stream OpenWrite(string container, string blobName, string fileName = null, long? length = null);

        void Delete(string container, string blobName);
    }
}
