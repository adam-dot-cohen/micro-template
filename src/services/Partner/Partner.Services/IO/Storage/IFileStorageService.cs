using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public interface IReadOnlyFileStorageService
    {
        bool Exists(StorageMoniker moniker);

        Stream OpenRead(StorageMoniker moniker);
    }

    public interface IFileStorageService : IReadOnlyFileStorageService
    {
        Stream OpenWrite(StorageMoniker moniker);

        void Delete(StorageMoniker moniker);
    }
}
