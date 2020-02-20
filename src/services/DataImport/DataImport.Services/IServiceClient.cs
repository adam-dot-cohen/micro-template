using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    public interface IServiceClient<TID, TDTO> where TDTO : Dto<TID>
    {
        Task<TDTO> GetAsync(TID id);
        Task<IEnumerable<TDTO>> GetAllAsync();
        Task<TID> CreateAsync(TDTO dto);
        Task UpdateAsync(TDTO dto);
        Task DeleteAsync(TDTO dto);
        Task DeleteAsync(TID id);
    }

    public abstract class WebServiceClientBase<TID, TDTO> : IServiceClient<TID, TDTO> where TDTO : Dto<TID>
    {
        protected abstract string ApiBasePath { get; set; }
        protected abstract string ResourcePath { get; set; }
        protected virtual string SearchPath => "search";

        public virtual async Task<TDTO> GetAsync(TID id)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, id)
                .GetJsonAsync<TDTO>();
        }    

        public virtual async Task<IEnumerable<TDTO>> GetAllAsync()
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, "search")
                .GetJsonAsync<IEnumerable<TDTO>>();
        }

        public virtual async Task<TID> CreateAsync(TDTO dto)
        {
            var response = await ApiBasePath
                .AppendPathSegment(ResourcePath)
                .PostJsonAsync(dto)
                .ReceiveJson<dynamic>();

            return response.id;
        }

        public virtual async Task UpdateAsync(TDTO dto)
        {
            await ApiBasePath
               .AppendPathSegments(ResourcePath, dto.Id)
               .PutJsonAsync(dto);
        }

        public virtual async Task DeleteAsync(TDTO dto)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, dto.Id)
             .DeleteAsync();
        }

        public virtual async Task DeleteAsync(TID id)
        {
            await ApiBasePath
             .AppendPathSegments(ResourcePath, id)
             .DeleteAsync();
        }
    }

    public abstract class DymmyServiceClientBase<TID, TDTO> : IServiceClient<TID, TDTO> where TDTO : Dto<TID>
    {
        protected abstract IEnumerable<TDTO> Dtos { get; }

        public virtual Task<TID> CreateAsync(TDTO dto)
        {
            return Task.FromResult<TID>(default);
        }

        public virtual Task DeleteAsync(TDTO dto)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task DeleteAsync(TID id)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task<IEnumerable<TDTO>> GetAllAsync()
        {
            return Task.FromResult(Dtos);
        }

        public virtual Task<TDTO> GetAsync(TID id)
        {
            var TDTO = Dtos.SingleOrDefault(p => p.Id.Equals(id));
            if (TDTO == null)
                throw new HttpRequestException($"404 {id} not found");

            return Task.FromResult(TDTO);
        }

        public virtual Task UpdateAsync(TDTO dto)
        {
            return Task.FromResult<object>(null);
        }
    }
}
