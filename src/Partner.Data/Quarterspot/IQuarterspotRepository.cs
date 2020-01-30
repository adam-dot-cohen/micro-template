using System.Collections.Generic;
using System.Threading.Tasks;

namespace Partner.Data.Quarterspot
{
    public interface IQuarterspotRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
    }
}
