using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services
{
    public interface IServiceClient<Tid, Tentity> where Tentity : TableStorageEntity
    {
        Task<Tentity> GetAsync(Tid id);
        Task<IEnumerable<Tentity>> GetAllAsync();
        Task<Tid> CreateAsync(Tentity dto);
        Task UpdateAsync(Tentity dto);
        Task DeleteAsync(Tentity dto);
        Task DeleteAsync(Tid id);
    }

    public abstract class WebServiceClientBase<Tid, Tentity> : IServiceClient<Tid, Tentity> where Tentity : TableStorageEntity
    {
        protected abstract string ApiBasePath { get; set; }
        protected abstract string ResourcePath { get; set; }
        protected virtual string SearchPath => "search";

        public virtual async Task<Tentity> GetAsync(Tid id)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, id)
                .GetJsonAsync<Tentity>();
        }    

        public virtual async Task<IEnumerable<Tentity>> GetAllAsync()
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, "search")
                .GetJsonAsync<IEnumerable<Tentity>>();
        }

        public virtual async Task<Tid> CreateAsync(Tentity dto)
        {
            var response = await ApiBasePath
                .AppendPathSegment(ResourcePath)
                .PostJsonAsync(dto)
                .ReceiveJson<dynamic>();

            return response.id;
        }

        public virtual async Task UpdateAsync(Tentity dto)
        {
            await ApiBasePath
               .AppendPathSegments(ResourcePath, dto.Id)
               .PutJsonAsync(dto);
        }

        public virtual async Task DeleteAsync(Tentity dto)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, dto.Id)
             .DeleteAsync();
        }

        public virtual async Task DeleteAsync(Tid id)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, id)
             .DeleteAsync();
        }
    }

    public abstract class DymmyServiceClientBase<Tid, Tentity> : IServiceClient<Tid, Tentity> where Tentity : TableStorageEntity
    {
        protected abstract IEnumerable<Tentity> Entities { get; }

        public virtual Task<Tid> CreateAsync(Tentity dto)
        {
            return Task.FromResult<Tid>(default);
        }

        public virtual Task DeleteAsync(Tentity dto)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task DeleteAsync(Tid id)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task<IEnumerable<Tentity>> GetAllAsync()
        {
            return Task.FromResult(Entities);
        }

        public virtual Task<Tentity> GetAsync(Tid id)
        {
            var dto = Entities.SingleOrDefault(p => p.Id.Equals(id));
            if (dto == null)
                throw new HttpRequestException($"404 {id} not found");

            return Task.FromResult(dto);
        }

        public virtual Task UpdateAsync(Tentity dto)
        {
            return Task.FromResult<object>(null);
        }
    }
}
