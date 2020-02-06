using System.IO;
using Partner.Core.IO;

namespace Partner.Services.IO.Storage.Blob.Azure
{
    public class FilSystemBlobStorageService : IBlobStorageService
    {
        protected static readonly string RootFolder = @"C:\Temp\CloudStorage";

        public bool Exists(string container, string blobName)
        {
            return File.Exists(FullPath(container, blobName));
        }

        public StreamStack OpenRead(string container, string blobName)
        {
            return new StreamStack(File.OpenRead(FullPath(container, blobName)));
        }

        protected string FullPath(string container, string blobName)
        {
            return Path.Combine(RootFolder, container, blobName);
        }

        public void Delete(string container, string blobName)
        {
            File.Delete(FullPath(container, blobName));
        }

        public StreamStack OpenWrite(string container, string blobName, string fileName = null, long? length = null, bool compress = false)
        {
            Directory.CreateDirectory(RootFolder);

            return new StreamStack(File.OpenWrite(FullPath(container, blobName)));
        }
    }
}
