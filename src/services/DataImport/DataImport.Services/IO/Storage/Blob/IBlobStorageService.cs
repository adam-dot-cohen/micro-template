using System.IO;
using DataImport.Core.IO;

namespace DataImport.Services.IO.Storage.Blob.Azure
{
    public interface IReadOnlyBlobStorageService
    {
        bool Exists(string container, string blobName);

        StreamStack OpenRead(string container, string blobName);
    }

    public interface IBlobStorageService : IReadOnlyBlobStorageService
    {
        StreamStack OpenWrite(string container, string blobName, string fileName = null, long? length = null, bool compress = false);

        void Delete(string container, string blobName);
    }
}
