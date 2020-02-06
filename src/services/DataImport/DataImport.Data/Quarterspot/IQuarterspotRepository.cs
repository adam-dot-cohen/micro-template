using System.Collections.Generic;
using System.Threading.Tasks;
using DataImport.Domain.Laso.Quarterspot.Models;

namespace DataImport.Data.Quarterspot
{
    public interface IQuarterspotRepository
    {
        Task<IEnumerable<QsCustomer>> GetCustomersAsync();
        Task<IEnumerable<QsCustomer>> GetCustomersAsync(int offset, int take);
        //Task<IEnumerable<QsFirmographic>> GetDemographicsAsync();
        //Task<IEnumerable<QsAccount>> GetAccountsAsync();

        Task<IEnumerable<QsBusiness>> GetBusinessesAsync();
        Task<IEnumerable<QsBusiness>> GetBusinessesAsync(int offset, int take);
    }
}
