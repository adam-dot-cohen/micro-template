using DataImport.Domain.Api;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataImport.Services
{
    public interface IServiceClient<TID, TDTO> where TDTO : ImportsDto<TID>
    {
        Task<TDTO> GetAsync(TID id);
        Task<IEnumerable<TDTO>> GetAllAsync();
        Task<TID> CreateAsync(TDTO dto);
        Task UpdateAsync(TDTO dto);
        Task DeleteAsync(TDTO dto);
        Task DeleteAsync(TID id);
    }

    public abstract class WebServiceClientBase<TID, TDTO> : IServiceClient<TID, TDTO> where TDTO : ImportsDto<TID>
    {
        protected abstract string ApiBasePath { get; set; }
        protected abstract string ResourcePath { get; set; }
        protected virtual string SearchPath => "search";

        public async Task<TDTO> GetAsync(TID id)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, id)
                .GetJsonAsync<TDTO>();
        }    

        public async Task<IEnumerable<TDTO>> GetAllAsync()
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, "search")
                .GetJsonAsync<IEnumerable<TDTO>>();
        }

        public async Task<TID> CreateAsync(TDTO TDTO)
        {
            var response = await ApiBasePath
                .AppendPathSegment(ResourcePath)
                .PostJsonAsync(TDTO)
                .ReceiveJson<dynamic>();

            return response.id;
        }

        public async Task UpdateAsync(TDTO TDTO)
        {
            await ApiBasePath
               .AppendPathSegments(ResourcePath, TDTO.Id)
               .PutJsonAsync(TDTO);
        }

        public async Task DeleteAsync(TDTO TDTO)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, TDTO.Id)
             .DeleteAsync();
        }

        public async Task DeleteAsync(TID id)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, id)
             .DeleteAsync();
        }
    }

    public abstract class DymmyServiceClientBase<TID, TDTO> : IServiceClient<TID, TDTO> where TDTO : ImportsDto<TID>
    {
        protected abstract IEnumerable<TDTO> DummyCollection { get; }

        public virtual Task<TID> CreateAsync(TDTO TDTO)
        {
            return Task.FromResult<TID>(default);
        }

        public virtual Task DeleteAsync(TDTO TDTO)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task DeleteAsync(TID id)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task<IEnumerable<TDTO>> GetAllAsync()
        {
            return Task.FromResult(DummyCollection);
        }

        public virtual Task<TDTO> GetAsync(TID id)
        {
            var TDTO = DummyCollection.SingleOrDefault(p => p.Id.Equals(id));
            if (TDTO == null)
                throw new HttpRequestException($"404 {id} not found");

            return Task.FromResult(TDTO);
        }

        public virtual Task UpdateAsync(TDTO TDTO)
        {
            return Task.FromResult<object>(null);
        }
    }
}
