namespace Laso.DataImport.Services.IO.Storage.Blob.Azure
{
    public interface IStorageMonikerFactory
    {
        StorageMoniker Create(StorageType type, string path, string name);
    }
}
