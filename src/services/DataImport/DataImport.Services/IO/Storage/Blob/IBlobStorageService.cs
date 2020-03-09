using System.IO;
using Laso.DataImport.Core.IO;

namespace Laso.DataImport.Services.IO.Storage.Blob.Azure
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
