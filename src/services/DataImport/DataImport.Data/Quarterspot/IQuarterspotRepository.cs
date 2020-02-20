using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.DataImport.Domain.Quarterspot.Models;

namespace Laso.DataImport.Data.Quarterspot
{
    public interface IQuarterspotRepository
    {
        Task<IEnumerable<QsCustomer>> GetCustomersAsync();
        Task<IEnumerable<QsCustomer>> GetCustomersAsync(int offset, int take);
        Task<IEnumerable<QsBusiness>> GetBusinessesAsync();
        Task<IEnumerable<QsBusiness>> GetBusinessesAsync(int offset, int take);
    }
}
