namespace Laso.Provisioning.Core.Persistence
{
    public interface IBlobStorageService
    {
        void CreateContainer(string name);
        void CreateDirectory(string containerName, string path);
        void WriteTextToFile(string containerName, string path, string text);
    }

    //TODO: remove both of these before April 30th
    public interface IColdBlobStorageService : IBlobStorageService
    {
    }

    //TODO: remove both of these before April 30th
    public interface IEscrowBlobStorageService : IBlobStorageService
    {
    }
}