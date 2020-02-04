using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public class StorageMonikerFactory : IStorageMonikerFactory
    {
        public StorageMoniker Create(StorageType type, string path, string name)
        {
            return type switch
            {
                StorageType.LocalFile => StorageMoniker.Parse(new Uri(Path.Combine(path, name), UriKind.Absolute).AbsoluteUri),
                StorageType.Http => throw new NotImplementedException(nameof(type)),
                StorageType.Blob => throw new NotImplementedException(nameof(type)),
                _ => throw new ArgumentOutOfRangeException(nameof(Type))
            };
        }
    }
}
