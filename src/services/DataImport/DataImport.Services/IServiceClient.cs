using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services
{
    public interface IServiceClient<Tid, Tdto> where Tdto : IDto<Tid>
    {
        Task<Tdto> GetAsync(Tid id);
        Task<IEnumerable<Tdto>> GetAllAsync();
        Task<Tid> CreateAsync(Tdto dto);
        Task UpdateAsync(Tdto dto);
        Task DeleteAsync(Tdto dto);
        Task DeleteAsync(Tid id);
    }

    public abstract class WebServiceClientBase<Tid, Tdto> : IServiceClient<Tid, Tdto> where Tdto : IDto<Tid>
    {
        protected abstract string ApiBasePath { get; set; }
        protected abstract string ResourcePath { get; set; }
        protected virtual string SearchPath => "search";

        public virtual async Task<Tdto> GetAsync(Tid id)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, id)
                .GetJsonAsync<Tdto>();
        }    

        public virtual async Task<IEnumerable<Tdto>> GetAllAsync()
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, "search")
                .GetJsonAsync<IEnumerable<Tdto>>();
        }

        public virtual async Task<Tid> CreateAsync(Tdto dto)
        {
            var response = await ApiBasePath
                .AppendPathSegment(ResourcePath)
                .PostJsonAsync(dto)
                .ReceiveJson<dynamic>();

            return response.id;
        }

        public virtual async Task UpdateAsync(Tdto dto)
        {
            await ApiBasePath
               .AppendPathSegments(ResourcePath, dto.Id)
               .PutJsonAsync(dto);
        }

        public virtual async Task DeleteAsync(Tdto dto)
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

    public abstract class DymmyServiceClientBase<Tid, Tdto> : IServiceClient<Tid, Tdto> where Tdto : IDto<Tid>
    {
        protected abstract IEnumerable<Tdto> Dtos { get; }

        public virtual Task<Tid> CreateAsync(Tdto dto)
        {
            return Task.FromResult<Tid>(default);
        }

        public virtual Task DeleteAsync(Tdto dto)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task DeleteAsync(Tid id)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task<IEnumerable<Tdto>> GetAllAsync()
        {
            return Task.FromResult(Dtos);
        }

        public virtual Task<Tdto> GetAsync(Tid id)
        {
            var dto = Dtos.SingleOrDefault(p => p.Id.Equals(id));
            if (dto == null)
                throw new HttpRequestException($"404 {id} not found");

            return Task.FromResult(dto);
        }

        public virtual Task UpdateAsync(Tdto dto)
        {
            return Task.FromResult<object>(null);
        }
    }
}
