using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public class ReadOnlyFileSystemBlobStorageService : IReadOnlyBlobStorageService
    {
        protected static readonly string RootFolder = @"C:\Temp\CloudStorage";

        public bool Exists(string container, string blobName)
        {
            return File.Exists(FullPath(container, blobName));
        }

        public Stream OpenRead(string container, string blobName)
        {
            return File.OpenRead(FullPath(container, blobName));
        }

        protected string FullPath(string container, string blobName)
        {
            return Path.Combine(RootFolder, container, blobName);
        }
    }

    public class FilSystemBlobStorageService : ReadOnlyFileSystemBlobStorageService, IBlobStorageService
    {
        public void Delete(string container, string blobName)
        {
            File.Delete(FullPath(container, blobName));
        }

        public Stream OpenWrite(string container, string blobName, string fileName = null, long? length = null)
        {
            Directory.CreateDirectory(RootFolder);

            return File.OpenWrite(FullPath(container, blobName));
        }
    }
}
