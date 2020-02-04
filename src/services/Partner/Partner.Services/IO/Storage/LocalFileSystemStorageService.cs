using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public class LocalFileSystemStorageService : IFileStorageService
    {
        public Stream OpenRead(StorageMoniker moniker)
        {
            return File.OpenRead(FullPath(moniker));
        }

        public Stream OpenWrite(StorageMoniker moniker)
        {
            return File.OpenWrite(FullPath(moniker));
        }

        public void Delete(StorageMoniker moniker)
        {
            File.Delete(FullPath(moniker));
        }

        public bool Exists(StorageMoniker moniker)
        {
            return File.Exists(FullPath(moniker));
        }

        private string FullPath(StorageMoniker moniker)
        {
            return moniker.LocalPath;
        }
    }
}
