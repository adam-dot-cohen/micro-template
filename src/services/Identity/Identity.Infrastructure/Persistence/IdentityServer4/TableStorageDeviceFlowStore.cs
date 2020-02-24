using System.Threading.Tasks;
using IdentityServer4.Stores;
using IdentityServerDeviceCode = IdentityServer4.Models.DeviceCode;

namespace Laso.Identity.Infrastructure.Persistence.IdentityServer4
{
    public class TableStorageDeviceFlowStore : IDeviceFlowStore
    {
        public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, IdentityServerDeviceCode data)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityServerDeviceCode> FindByUserCodeAsync(string userCode)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityServerDeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateByUserCodeAsync(string userCode, IdentityServerDeviceCode data)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
