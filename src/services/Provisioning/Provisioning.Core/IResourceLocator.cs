using System.Threading.Tasks;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core
{
    public interface IResourceLocator
    {
        Task<string> GetLocationString(ProvisionedResourceEvent provisionedResource);
    }
}