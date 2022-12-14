using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Laso.TableStorage;
using IdentityServerPersistedGrant = IdentityServer4.Models.PersistedGrant;

namespace Laso.Identity.Infrastructure.Persistence
{
    public class TableStoragePersistedGrantStore : IPersistedGrantStore
    {
        private readonly ITableStorageService _tableStorageService;

        public TableStoragePersistedGrantStore(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task StoreAsync(IdentityServerPersistedGrant grant)
        {
            await _tableStorageService.InsertOrReplaceAsync(new PersistedGrant
            {
                Key = grant.Key,
                Type = grant.Type,
                SubjectId = grant.SubjectId,
                ClientId = grant.ClientId,
                CreationTime = grant.CreationTime,
                Expiration = grant.Expiration,
                Data = grant.Data
            });
        }

        public async Task<IdentityServerPersistedGrant> GetAsync(string key)
        {
            var grant = await _tableStorageService.GetAsync<PersistedGrant>(key);

            return MapGrant(grant);
        }

        public Task<IEnumerable<IdentityServerPersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<IdentityServerPersistedGrant>> GetAllAsync(string subjectId)
        {
            var grants = await _tableStorageService.FindAllAsync<PersistedGrant>(x => x.SubjectId == subjectId);

            return grants.Select(MapGrant);
        }

        public async Task RemoveAsync(string key)
        {
            var id = await _tableStorageService.GetAsync<PersistedGrant>(key);
            await _tableStorageService.DeleteAsync(id);
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var grants = await _tableStorageService.FindAllAsync<PersistedGrant>(x => x.SubjectId == subjectId && x.ClientId == clientId);

            await _tableStorageService.DeleteAsync(grants);
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var grants = await _tableStorageService.FindAllAsync<PersistedGrant>(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type);

            await _tableStorageService.DeleteAsync(grants);
        }

        private static IdentityServerPersistedGrant MapGrant(PersistedGrant grant)
        {
            return new IdentityServerPersistedGrant
            {
                Key = grant.Key,
                Type = grant.Type,
                SubjectId = grant.SubjectId,
                ClientId = grant.ClientId,
                CreationTime = grant.CreationTime,
                Expiration = grant.Expiration,
                Data = grant.Data
            };
        }
    }
}