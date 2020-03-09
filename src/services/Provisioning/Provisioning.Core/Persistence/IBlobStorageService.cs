namespace Laso.Provisioning.Core.Persistence
{
    public interface IBlobStorageService
    {
        void CreateContainer(string name);
    }
}