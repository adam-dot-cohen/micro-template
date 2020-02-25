using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Infrastructure.Extensions;
using Laso.Identity.Infrastructure.Persistence.IdentityServer4.Entities;
using ApiResource = Laso.Identity.Infrastructure.Persistence.IdentityServer4.Entities.ApiResource;
using IdentityResource = Laso.Identity.Infrastructure.Persistence.IdentityServer4.Entities.IdentityResource;
using IdentityServerIdentityResource = IdentityServer4.Models.IdentityResource;
using IdentityServerApiResource = IdentityServer4.Models.ApiResource;
using IdentityServerSecret = IdentityServer4.Models.Secret;
using IdentityServerScope = IdentityServer4.Models.Scope;

namespace Laso.Identity.Infrastructure.Persistence.IdentityServer4
{
    public class TableStorageResourceStore : IResourceStore
    {
        private readonly ITableStorageService _tableStorageService;

        public TableStorageResourceStore(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<IdentityServerIdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            Expression<Func<IdentityResource, bool>> filter = x => false;

            scopeNames.ForEach(x => filter = filter.Or(y => y.PartitionKey == x));

            var resources = await _tableStorageService.FindAllAsync(filter);

            return resources.Select(MapIdentityResource);
        }

        public async Task<IEnumerable<IdentityServerApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            Expression<Func<ApiScope, bool>> scopeFilter = x => false;

            scopeNames.ForEach(x => scopeFilter = scopeFilter.Or(y => y.PartitionKey == x));

            var scopes = await _tableStorageService.FindAllAsync(scopeFilter);

            Expression<Func<ApiResource, bool>> resourceFilter = x => false;
            Expression<Func<ApiSecret, bool>> secretFilter = x => false;

            scopes.Select(x => x.ApiResourceName).Distinct().ForEach(x =>
            {
                resourceFilter = resourceFilter.Or(y => y.PartitionKey == x);
                secretFilter = secretFilter.Or(y => y.PartitionKey == x);
            });

            var resources = await _tableStorageService.FindAllAsync(resourceFilter);
            var secrets = await _tableStorageService.FindAllAsync(secretFilter);

            return resources.Select(x => MapApiResource(x, secrets.Where(y => y.ApiResourceName == x.Name), scopes.Where(y => y.ApiResourceName == x.Name)));
        }

        public async Task<IdentityServerApiResource> FindApiResourceAsync(string name)
        {
            var resource = await _tableStorageService.GetAsync<ApiResource>(name);
            var secrets = await _tableStorageService.GetAllAsync<ApiSecret>(name);
            var scopes = await _tableStorageService.FindAllAsync<ApiScope>(x => x.ApiResourceName == name);

            return MapApiResource(resource, secrets, scopes);
        }

        public async Task<global::IdentityServer4.Models.Resources> GetAllResourcesAsync()
        {
            var apiResources = await _tableStorageService.GetAllAsync<ApiResource>();
            var secrets = (await _tableStorageService.GetAllAsync<ApiSecret>()).ToLookup(x => x.ApiResourceName);
            var scopes = (await _tableStorageService.GetAllAsync<ApiScope>()).ToLookup(x => x.ApiResourceName);
            var identityResources = await _tableStorageService.GetAllAsync<IdentityResource>();

            return new global::IdentityServer4.Models.Resources
            {
                ApiResources = apiResources.Select(x => MapApiResource(x, secrets[x.Name], scopes[x.Name])).ToList(),
                IdentityResources = identityResources.Select(MapIdentityResource).ToList()
            };
        }

        private static IdentityServerIdentityResource MapIdentityResource(IdentityResource resource)
        {
            return new IdentityServerIdentityResource
            {
                Enabled = resource.Enabled,
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                UserClaims = resource.UserClaims,
                Properties = resource.Properties,
                Required = resource.Required,
                Emphasize = resource.Emphasize,
                ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument
            };
        }

        private static IdentityServerApiResource MapApiResource(ApiResource resource, IEnumerable<ApiSecret> secrets, IEnumerable<ApiScope> scopes)
        {
            return new IdentityServerApiResource
            {
                Enabled = resource.Enabled,
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                UserClaims = resource.UserClaims,
                Properties = resource.Properties,
                ApiSecrets = secrets.Select(MapSecret).ToList(),
                Scopes = scopes.Select(MapScope).ToList()
            };
        }

        private static IdentityServerSecret MapSecret(ApiSecret secret)
        {
            return new IdentityServerSecret
            {
                Description = secret.Description,
                Value = secret.Value,
                Expiration = secret.Expiration,
                Type = secret.Type
            };
        }

        private static IdentityServerScope MapScope(ApiScope scope)
        {
            return new IdentityServerScope
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Required = scope.Required,
                Emphasize = scope.Emphasize,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                UserClaims = scope.UserClaims
            };
        }
    }
}
