namespace Partner.Services.IO.Storage
{
    public interface IStorageMonikerFactory
    {
        StorageMoniker Create(StorageType type, string path, string name);
    }
}
